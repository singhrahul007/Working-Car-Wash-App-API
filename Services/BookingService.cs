using CarWash.Api.Data;
using CarWash.Api.DTOs;
using CarWash.Api.Models.Entities;
using CarWash.Api.Interfaces;
using CarWash.Api.Services.Interfaces;
using CarWash.Api.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;
    private readonly IServiceService _serviceService;
    private readonly IOfferService _offerService;
    private readonly IEmailService _emailService;

    public BookingService(
        AppDbContext context,
        IServiceService serviceService,
        IOfferService offerService,
        IEmailService emailService)
    {
        _context = context;
        _serviceService = serviceService;
        _offerService = offerService;
        _emailService = emailService;
    }

    public async Task<ServiceResult<BookingDto>> CreateBookingAsync(Guid userId, BookingCreateDto bookingDto)
    {
        try
        {
            // Get service
            var serviceResult = await _serviceService.GetServiceByIdAsync(bookingDto.ServiceId);
            if (!serviceResult.Success)
                return ServiceResult<BookingDto>.FailureResult(serviceResult.Message);

            var service = serviceResult.Data;

            // Check availability
            var isAvailable = await _serviceService.IsTimeSlotAvailableAsync(
                bookingDto.ServiceId,
                bookingDto.ScheduledDate,
                bookingDto.ScheduledTime);

            if (!isAvailable.Data)
                return ServiceResult<BookingDto>.FailureResult("Selected time slot is not available");

            // Handle address
            Address address = null;
            if (bookingDto.AddressId.HasValue)
            {
                address = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.Id == bookingDto.AddressId && a.UserId == userId);
            }
            else if (bookingDto.NewAddress != null)
            {
                address = new Address
                {
                    UserId = userId,
                    FullAddress = bookingDto.NewAddress.FullAddress,
                    City = bookingDto.NewAddress.City,
                    State = bookingDto.NewAddress.State,
                    Country = bookingDto.NewAddress.Country,
                    PostalCode = bookingDto.NewAddress.PostalCode,
                    Latitude = bookingDto.NewAddress.Latitude,
                    Longitude = bookingDto.NewAddress.Longitude,
                    IsDefault = bookingDto.NewAddress.IsDefault,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Addresses.AddAsync(address);
                await _context.SaveChangesAsync();
            }

            // Calculate price
            var subtotal = service.Price;
            var taxAmount = subtotal * 0.18m; // 18% GST
            var discountAmount = 0m;

            // Apply offer if provided
            if (!string.IsNullOrEmpty(bookingDto.AppliedOfferCode))
            {
                var categories = new List<string> { service.Category };
                var offerResult = await _offerService.ValidateAndApplyOfferAsync(
                    bookingDto.AppliedOfferCode,
                    subtotal,
                    categories);

                if (offerResult.Success)
                {
                    discountAmount = offerResult.Data.DiscountAmount;
                }
            }

            var totalAmount = subtotal + taxAmount - discountAmount;

            // Generate booking ID
            var bookingId = GenerateBookingId();

            // Create booking
            var booking = new Booking
            {
                UserId = userId,
                ServiceId = bookingDto.ServiceId,
                AddressId = address?.Id,
                BookingId = bookingId,
                VehicleType = bookingDto.VehicleType,
                ACType = bookingDto.ACType,
                ACBrand = bookingDto.ACBrand,
                SofaType = bookingDto.SofaType,
                SofaCount = bookingDto.SofaCount,
                ScheduledDate = bookingDto.ScheduledDate,
                ScheduledTime = bookingDto.ScheduledTime,
                Status = "confirmed",
                Subtotal = subtotal,
                DiscountAmount = discountAmount,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount,
                PaymentStatus = "pending",
                PaymentMethod = bookingDto.PaymentMethod ?? "cash",
                AppliedOfferCode = bookingDto.AppliedOfferCode,
                SpecialInstructions = bookingDto.SpecialInstructions,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();

            // Send confirmation email
            var user = await _context.Users.FindAsync(userId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendBookingConfirmationAsync(
                    user.Email,
                    booking.BookingId,
                    service.Name,
                    booking.ScheduledDate);
            }

            var bookingDtoResult = await MapToBookingDtoAsync(booking);
            return ServiceResult<BookingDto>.SuccessResult(bookingDtoResult, "Booking created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<BookingDto>.FailureResult($"Failed to create booking: {ex.Message}");
        }
    }

    private string GenerateBookingId()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = new Random().Next(1000, 9999).ToString("D4");
        return $"CW-{datePart}-{randomPart}";
    }

    public async Task<ServiceResult<List<BookingDto>>> GetUserBookingsAsync(Guid userId, string status = null)
    {
        try
        {
            var query = _context.Bookings
                .Include(b => b.Service)
                .Include(b => b.Address)
                .Where(b => b.UserId == userId);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.Status == status);
            }

            var bookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var bookingDtos = new List<BookingDto>();
            foreach (var booking in bookings)
            {
                var bookingDto = await MapToBookingDtoAsync(booking);
                bookingDtos.Add(bookingDto);
            }

            return ServiceResult<List<BookingDto>>.SuccessResult(bookingDtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<BookingDto>>.FailureResult($"Failed to get bookings: {ex.Message}");
        }
    }

    public async Task<ServiceResult<BookingDto>> GetBookingByIdAsync(string bookingId, Guid userId)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Service)
                .Include(b => b.Address)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);

            if (booking == null)
                return ServiceResult<BookingDto>.FailureResult("Booking not found");

            var bookingDto = await MapToBookingDtoAsync(booking);
            return ServiceResult<BookingDto>.SuccessResult(bookingDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<BookingDto>.FailureResult($"Failed to get booking: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> CancelBookingAsync(string bookingId, Guid userId)
    {
        try
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);

            if (booking == null)
                return ServiceResult<bool>.FailureResult("Booking not found");

            // Check if booking can be cancelled
            if (booking.Status == "completed" || booking.Status == "cancelled")
                return ServiceResult<bool>.FailureResult($"Booking is already {booking.Status}");

            // Allow cancellation up to 2 hours before scheduled time
            var scheduledDateTime = booking.ScheduledDate.Date.Add(
                TimeSpan.Parse(booking.ScheduledTime));

            if (DateTime.UtcNow.AddHours(2) > scheduledDateTime)
                return ServiceResult<bool>.FailureResult("Cannot cancel booking within 2 hours of scheduled time");

            booking.Status = "cancelled";
            booking.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true, "Booking cancelled successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Failed to cancel booking: {ex.Message}");
        }
    }

    public async Task<ServiceResult<CartDto>> GetCartAsync(Guid userId)
    {
        try
        {
            // In a real app, cart might be stored in Redis or database
            var cart = new CartDto
            {
                Items = new List<CartItemDetailDto>(),
                Subtotal = 0,
                DiscountAmount = 0,
                TaxAmount = 0,
                TotalAmount = 0
            };

            return ServiceResult<CartDto>.SuccessResult(cart);
        }
        catch (Exception ex)
        {
            return ServiceResult<CartDto>.FailureResult($"Failed to get cart: {ex.Message}");
        }
    }

    // Implement missing cart methods with Guid userId
    public Task<ServiceResult<CartDto>> AddToCartAsync(Guid userId, CartItemDto cartItem)
    {
        // Implementation for adding to cart
        throw new NotImplementedException();
    }

    public Task<ServiceResult<CartDto>> RemoveFromCartAsync(Guid userId, int serviceId)
    {
        // Implementation for removing from cart
        throw new NotImplementedException();
    }

    public Task<ServiceResult<CartDto>> UpdateCartItemAsync(Guid userId, CartItemDto cartItem)
    {
        // Implementation for updating cart item
        throw new NotImplementedException();
    }

    public Task<ServiceResult<CartDto>> ApplyOfferToCartAsync(Guid userId, string offerCode)
    {
        // Implementation for applying offer to cart
        throw new NotImplementedException();
    }

    public Task<ServiceResult<CartDto>> RemoveOfferFromCartAsync(Guid userId)
    {
        // Implementation for removing offer from cart
        throw new NotImplementedException();
    }

    public Task<ServiceResult<decimal>> CalculateCartTotalAsync(Guid userId)
    {
        // Implementation for calculating cart total
        throw new NotImplementedException();
    }

    private async Task<BookingDto> MapToBookingDtoAsync(Booking booking)
    {
        var serviceDto = await _serviceService.GetServiceByIdAsync(booking.ServiceId);
        AddressDto addressDto = null;

        if (booking.AddressId.HasValue)
        {
            var address = await _context.Addresses.FindAsync(booking.AddressId.Value);
            if (address != null)
            {
                addressDto = new AddressDto
                {
                    Id = address.Id,
                    FullAddress = address.FullAddress,
                    City = address.City,
                    State = address.State,
                    Country = address.Country,
                    PostalCode = address.PostalCode,
                    IsDefault = address.IsDefault
                };
            }
        }

        return new BookingDto
        {
            BookingId = booking.BookingId,
            Service = serviceDto.Data,
            Status = booking.Status,
            ScheduledDate = booking.ScheduledDate,
            ScheduledTime = booking.ScheduledTime,
            TotalAmount = booking.TotalAmount,
            CreatedAt = booking.CreatedAt,
            VehicleType = booking.VehicleType,
            Address = addressDto,
            ACType = booking.ACType,
            ACBrand = booking.ACBrand,
            SofaType = booking.SofaType,
            SofaCount = booking.SofaCount,
            PaymentStatus = booking.PaymentStatus,
            PaymentMethod = booking.PaymentMethod
        };
    }
}