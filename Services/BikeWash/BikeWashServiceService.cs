using CarWash.Api.Data;
using CarWash.Api.Models.Entities;
using CarWash.Api.Models.Entities.BikeWash;
using CarWash.Api.Models.DTOs.BikeWash;
using CarWash.Api.Services.Interfaces.BikeWash;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarWash.Api.Services.BikeWash
{

    public class BikeWashServiceService : IBikeWashServiceService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BikeWashServiceService> _logger;

        public BikeWashServiceService(AppDbContext context, ILogger<BikeWashServiceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ─── Services ────────────────────────────────────────────────────────────

        public async Task<BikeWashServiceResponse<List<BikeWashServiceDTOs>>> GetAllServicesAsync(BikeWashServiceFilterDTOs? filter = null)
        {
            try
            {
                var query = _context.BikeWashServices.Where(s => s.IsActive);

                if (!string.IsNullOrEmpty(filter?.Category))
                    query = query.Where(s => s.Category == filter.Category);

                if (filter?.IsPopular.HasValue == true)
                    query = query.Where(s => s.IsPopular == filter.IsPopular);

                var services = await query
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Name)
                    .ToListAsync();

                return new BikeWashServiceResponse<List<BikeWashServiceDTOs>>
                {
                    Success = true,
                    Data = services.Select(MapToBikeWashServiceDto).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Bike Wash services");
                return new BikeWashServiceResponse<List<BikeWashServiceDTOs>> { Success = false, Message = "Error retrieving services" };
            }
        }

        public async Task<BikeWashServiceResponse<BikeWashServiceDTOs>> GetServiceByIdAsync(int id)
        {
            try
            {
                var service = await _context.BikeWashServices.FindAsync(id);
                if (service == null || !service.IsActive)
                    return new BikeWashServiceResponse<BikeWashServiceDTOs> { Success = false, Message = "Service not found" };

                return new BikeWashServiceResponse<BikeWashServiceDTOs> { Success = true, Data = MapToBikeWashServiceDto(service) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Bike Wash service by ID: {Id}", id);
                return new BikeWashServiceResponse<BikeWashServiceDTOs> { Success = false, Message = "Error retrieving service" };
            }
        }

        public async Task<BikeWashServiceResponse<List<BikeWashServiceDTOs>>> GetServicesByCategoryAsync(string category)
        {
            try
            {
                var services = await _context.BikeWashServices
                    .Where(s => s.IsActive && s.Category == category)
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Name)
                    .ToListAsync();

                return new BikeWashServiceResponse<List<BikeWashServiceDTOs>>
                {
                    Success = true,
                    Data = services.Select(MapToBikeWashServiceDto).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Bike Wash services by category: {Category}", category);
                return new BikeWashServiceResponse<List<BikeWashServiceDTOs>> { Success = false, Message = "Error retrieving services" };
            }
        }

        public async Task<BikeWashServiceResponse<List<BikeWashServiceDTOs>>> GetPopularServicesAsync()
        {
            try
            {
                var services = await _context.BikeWashServices
                    .Where(s => s.IsActive && s.IsPopular)
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Name)
                    .Take(10)
                    .ToListAsync();

                return new BikeWashServiceResponse<List<BikeWashServiceDTOs>>
                {
                    Success = true,
                    Data = services.Select(MapToBikeWashServiceDto).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular Bike Wash services");
                return new BikeWashServiceResponse<List<BikeWashServiceDTOs>> { Success = false, Message = "Error retrieving services" };
            }
        }

        // ─── Bookings ─────────────────────────────────────────────────────────────

        public async Task<BikeWashServiceResponse<BikeWashBookingResponseDTOs>> CreateBookingAsync(BikeWashBookingCreateDTOs bookingDto, Guid userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate service IDs
                var services = await _context.BikeWashServices
                    .Where(s => bookingDto.ServiceIds.Contains(s.Id) && s.IsActive)
                    .ToListAsync();

                if (services.Count != bookingDto.ServiceIds.Count)
                    return new BikeWashServiceResponse<BikeWashBookingResponseDTOs>
                    {
                        Success = false,
                        Message = "One or more services not found or inactive"
                    };

                // Calculate total
                var totalAmount = services.Sum(s => s.Price);

                // Build service items for JSON storage
                var serviceItems = services.Select(s => new BikeWashBookingServiceItem
                {
                    ServiceId = s.Id,
                    ServiceName = s.Name,
                    Price = s.Price,
                    Duration = s.DurationDisplay,
                    Includes = s.Includes
                }).ToList();

                // Create the booking
                var booking = new BikeWashBooking
                {
                    UserId = userId,
                    CustomerPhone = bookingDto.CustomerPhone,
                    CustomerAddress = bookingDto.CustomerAddress,
                    BikeType = bookingDto.BikeType,
                    ScheduledDate = bookingDto.ScheduledDate.Date,
                    ScheduledTime = bookingDto.ScheduledTime,
                    TotalAmount = totalAmount,
                    SpecialInstructions = bookingDto.SpecialInstructions ?? string.Empty,
                    Status = "confirmed",
                    PaymentStatus = "pending",
                    CreatedAt = DateTime.UtcNow,
                    SelectedServices = JsonSerializer.Serialize(serviceItems),
                    ServiceItems = serviceItems
                };

                _context.BikeWashBookings.Add(booking);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new BikeWashServiceResponse<BikeWashBookingResponseDTOs>
                {
                    Success = true,
                    Message = "Bike Wash booking created successfully",
                    Data = new BikeWashBookingResponseDTOs
                    {
                        Booking = MapToBikeWashBookingDto(booking),
                        BookingId = booking.BookingId,
                        TotalAmount = totalAmount
                    }
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating Bike Wash booking for user {UserId}", userId);
                return new BikeWashServiceResponse<BikeWashBookingResponseDTOs> { Success = false, Message = "Error creating booking. Please try again." };
            }
        }

        public async Task<BikeWashServiceResponse<List<BikeWashBookingDTOs>>> GetUserBookingsAsync(Guid userId)
        {
            try
            {
                var bookings = await _context.BikeWashBookings
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                return new BikeWashServiceResponse<List<BikeWashBookingDTOs>>
                {
                    Success = true,
                    Data = bookings.Select(MapToBikeWashBookingDto).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Bike Wash bookings for user {UserId}", userId);
                return new BikeWashServiceResponse<List<BikeWashBookingDTOs>> { Success = false, Message = "Error retrieving bookings" };
            }
        }

        public async Task<BikeWashServiceResponse<BikeWashBookingDTOs>> GetBookingByIdAsync(int bookingId, Guid userId)
        {
            try
            {
                var booking = await _context.BikeWashBookings
                    .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

                if (booking == null)
                    return new BikeWashServiceResponse<BikeWashBookingDTOs> { Success = false, Message = "Booking not found" };

                return new BikeWashServiceResponse<BikeWashBookingDTOs> { Success = true, Data = MapToBikeWashBookingDto(booking) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Bike Wash booking {BookingId}", bookingId);
                return new BikeWashServiceResponse<BikeWashBookingDTOs> { Success = false, Message = "Error retrieving booking" };
            }
        }

        public async Task<BikeWashServiceResponse<bool>> CancelBookingAsync(int bookingId, Guid userId)
        {
            try
            {
                var booking = await _context.BikeWashBookings
                    .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

                if (booking == null)
                    return new BikeWashServiceResponse<bool> { Success = false, Message = "Booking not found" };

                if (booking.Status is "completed" or "cancelled")
                    return new BikeWashServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Booking cannot be cancelled. Current status: {booking.Status}"
                    };

                booking.Status = "cancelled";
                booking.UpdatedAt = DateTime.UtcNow;
                _context.BikeWashBookings.Update(booking);
                await _context.SaveChangesAsync();

                return new BikeWashServiceResponse<bool> { Success = true, Message = "Booking cancelled successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling Bike Wash booking {BookingId}", bookingId);
                return new BikeWashServiceResponse<bool> { Success = false, Message = "Error cancelling booking" };
            }
        }

        public async Task<BikeWashServiceResponse<bool>> UpdateBookingStatusAsync(int bookingId, string status)
        {
            try
            {
                var booking = await _context.BikeWashBookings.FindAsync(bookingId);
                if (booking == null)
                    return new BikeWashServiceResponse<bool> { Success = false, Message = "Booking not found" };

                booking.Status = status;
                booking.UpdatedAt = DateTime.UtcNow;
                if (status == "completed")
                    booking.CompletedAt = DateTime.UtcNow;

                _context.BikeWashBookings.Update(booking);
                await _context.SaveChangesAsync();

                return new BikeWashServiceResponse<bool> { Success = true, Message = $"Booking status updated to {status}" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Bike Wash booking status {BookingId}", bookingId);
                return new BikeWashServiceResponse<bool> { Success = false, Message = "Error updating booking status" };
            }
        }

        // ─── Private helpers ──────────────────────────────────────────────────────

        private BikeWashServiceDTOs MapToBikeWashServiceDto(BikeWashService service)
        {
            var includes = JsonSerializer.Deserialize<List<string>>(service.Includes) ?? new List<string>();
            return new BikeWashServiceDTOs
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
                Category = service.Category,
                Price = service.Price,
                DurationInMinutes = service.DurationInMinutes,
                DurationDisplay = service.DurationDisplay,
                Includes = includes,
                IsPopular = service.IsPopular,
                DisplayOrder = service.DisplayOrder
            };
        }

        private BikeWashBookingDTOs MapToBikeWashBookingDto(BikeWashBooking booking)
        {
            var serviceItems = JsonSerializer.Deserialize<List<BikeWashBookingServiceItem>>(booking.SelectedServices)
                ?? new List<BikeWashBookingServiceItem>();

            return new BikeWashBookingDTOs
            {
                Id = booking.Id,
                BookingId = booking.BookingId,
                UserId = booking.UserId,
                CustomerPhone = booking.CustomerPhone,
                CustomerAddress = booking.CustomerAddress,
                BikeType = booking.BikeType,
                ScheduledDate = booking.ScheduledDate,
                ScheduledTime = booking.ScheduledTime,
                Status = booking.Status,
                TotalAmount = booking.TotalAmount,
                PaymentStatus = booking.PaymentStatus,
                SpecialInstructions = booking.SpecialInstructions,
                CreatedAt = booking.CreatedAt,
                ServiceItems = serviceItems
            };
        }

        /// <summary>
        /// Attempt to mirror the booking to the legacy Bookings table so OrdersScreen can display it.
        /// </summary>
    }
}
