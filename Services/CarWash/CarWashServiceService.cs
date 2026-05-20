using CarWash.Api.Data;
using CarWash.Api.Models.Entities;
using CarWash.Api.Models.Entities.CarWash;
using CarWash.Api.Models.DTOs.CarWash;
using CarWash.Api.Services.Interfaces.CarWash;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarWash.Api.Services.CarWash
{

    public class CarWashServiceService : ICarWashServiceService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CarWashServiceService> _logger;

        public CarWashServiceService(AppDbContext context, ILogger<CarWashServiceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ─── Services ────────────────────────────────────────────────────────────

        public async Task<CarWashServiceResponse<List<CarWashServiceDTOs>>> GetAllServicesAsync(CarWashServiceFilterDTOs? filter = null)
        {
            try
            {
                var query = _context.CarWashServices.Where(s => s.IsActive);

                if (!string.IsNullOrEmpty(filter?.Category))
                    query = query.Where(s => s.Category == filter.Category);

                if (filter?.IsPopular.HasValue == true)
                    query = query.Where(s => s.IsPopular == filter.IsPopular);

                var services = await query
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Name)
                    .ToListAsync();

                return new CarWashServiceResponse<List<CarWashServiceDTOs>>
                {
                    Success = true,
                    Data = services.Select(MapToCarWashServiceDto).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Car Wash services");
                return new CarWashServiceResponse<List<CarWashServiceDTOs>> { Success = false, Message = "Error retrieving services" };
            }
        }

        public async Task<CarWashServiceResponse<CarWashServiceDTOs>> GetServiceByIdAsync(int id)
        {
            try
            {
                var service = await _context.CarWashServices.FindAsync(id);
                if (service == null || !service.IsActive)
                    return new CarWashServiceResponse<CarWashServiceDTOs> { Success = false, Message = "Service not found" };

                return new CarWashServiceResponse<CarWashServiceDTOs> { Success = true, Data = MapToCarWashServiceDto(service) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Car Wash service by ID: {Id}", id);
                return new CarWashServiceResponse<CarWashServiceDTOs> { Success = false, Message = "Error retrieving service" };
            }
        }

        public async Task<CarWashServiceResponse<List<CarWashServiceDTOs>>> GetServicesByCategoryAsync(string category)
        {
            try
            {
                var services = await _context.CarWashServices
                    .Where(s => s.IsActive && s.Category == category)
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Name)
                    .ToListAsync();

                return new CarWashServiceResponse<List<CarWashServiceDTOs>>
                {
                    Success = true,
                    Data = services.Select(MapToCarWashServiceDto).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Car Wash services by category: {Category}", category);
                return new CarWashServiceResponse<List<CarWashServiceDTOs>> { Success = false, Message = "Error retrieving services" };
            }
        }

        public async Task<CarWashServiceResponse<List<CarWashServiceDTOs>>> GetPopularServicesAsync()
        {
            try
            {
                var services = await _context.CarWashServices
                    .Where(s => s.IsActive && s.IsPopular)
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Name)
                    .Take(10)
                    .ToListAsync();

                return new CarWashServiceResponse<List<CarWashServiceDTOs>>
                {
                    Success = true,
                    Data = services.Select(MapToCarWashServiceDto).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular Car Wash services");
                return new CarWashServiceResponse<List<CarWashServiceDTOs>> { Success = false, Message = "Error retrieving services" };
            }
        }

        // ─── Bookings ─────────────────────────────────────────────────────────────

        public async Task<CarWashServiceResponse<CarWashBookingResponseDTOs>> CreateBookingAsync(CarWashBookingCreateDTOs bookingDto, Guid userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate service IDs
                var services = await _context.CarWashServices
                    .Where(s => bookingDto.ServiceIds.Contains(s.Id) && s.IsActive)
                    .ToListAsync();

                if (services.Count != bookingDto.ServiceIds.Count)
                    return new CarWashServiceResponse<CarWashBookingResponseDTOs>
                    {
                        Success = false,
                        Message = "One or more services not found or inactive"
                    };

                // Calculate total
                var totalAmount = services.Sum(s => s.Price);

                // Build service items for JSON storage
                var serviceItems = services.Select(s => new CarWashBookingServiceItem
                {
                    ServiceId = s.Id,
                    ServiceName = s.Name,
                    Price = s.Price,
                    Duration = s.DurationDisplay,
                    Includes = s.Includes
                }).ToList();

                // Create the booking
                var booking = new CarWashBooking
                {
                    UserId = userId,
                    CustomerPhone = bookingDto.CustomerPhone,
                    CustomerAddress = bookingDto.CustomerAddress,
                    VehicleSize = bookingDto.VehicleSize,
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

                _context.CarWashBookings.Add(booking);
                await _context.SaveChangesAsync();

                // Mirror to legacy Bookings table for OrdersScreen compatibility
                await SaveToBookingHistoryAsync(booking);

                await transaction.CommitAsync();

                return new CarWashServiceResponse<CarWashBookingResponseDTOs>
                {
                    Success = true,
                    Message = "Car Wash booking created successfully",
                    Data = new CarWashBookingResponseDTOs
                    {
                        Booking = MapToCarWashBookingDto(booking),
                        BookingId = booking.BookingId,
                        TotalAmount = totalAmount
                    }
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating Car Wash booking for user {UserId}", userId);
                return new CarWashServiceResponse<CarWashBookingResponseDTOs> { Success = false, Message = "Error creating booking. Please try again." };
            }
        }

        public async Task<CarWashServiceResponse<List<CarWashBookingDTOs>>> GetUserBookingsAsync(Guid userId)
        {
            try
            {
                var bookings = await _context.CarWashBookings
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                return new CarWashServiceResponse<List<CarWashBookingDTOs>>
                {
                    Success = true,
                    Data = bookings.Select(MapToCarWashBookingDto).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Car Wash bookings for user {UserId}", userId);
                return new CarWashServiceResponse<List<CarWashBookingDTOs>> { Success = false, Message = "Error retrieving bookings" };
            }
        }

        public async Task<CarWashServiceResponse<CarWashBookingDTOs>> GetBookingByIdAsync(int bookingId, Guid userId)
        {
            try
            {
                var booking = await _context.CarWashBookings
                    .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

                if (booking == null)
                    return new CarWashServiceResponse<CarWashBookingDTOs> { Success = false, Message = "Booking not found" };

                return new CarWashServiceResponse<CarWashBookingDTOs> { Success = true, Data = MapToCarWashBookingDto(booking) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Car Wash booking {BookingId}", bookingId);
                return new CarWashServiceResponse<CarWashBookingDTOs> { Success = false, Message = "Error retrieving booking" };
            }
        }

        public async Task<CarWashServiceResponse<bool>> CancelBookingAsync(int bookingId, Guid userId)
        {
            try
            {
                var booking = await _context.CarWashBookings
                    .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

                if (booking == null)
                    return new CarWashServiceResponse<bool> { Success = false, Message = "Booking not found" };

                if (booking.Status is "completed" or "cancelled")
                    return new CarWashServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Booking cannot be cancelled. Current status: {booking.Status}"
                    };

                booking.Status = "cancelled";
                booking.UpdatedAt = DateTime.UtcNow;
                _context.CarWashBookings.Update(booking);
                await _context.SaveChangesAsync();

                return new CarWashServiceResponse<bool> { Success = true, Message = "Booking cancelled successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling Car Wash booking {BookingId}", bookingId);
                return new CarWashServiceResponse<bool> { Success = false, Message = "Error cancelling booking" };
            }
        }

        public async Task<CarWashServiceResponse<bool>> UpdateBookingStatusAsync(int bookingId, string status)
        {
            try
            {
                var booking = await _context.CarWashBookings.FindAsync(bookingId);
                if (booking == null)
                    return new CarWashServiceResponse<bool> { Success = false, Message = "Booking not found" };

                booking.Status = status;
                booking.UpdatedAt = DateTime.UtcNow;
                if (status == "completed")
                    booking.CompletedAt = DateTime.UtcNow;

                _context.CarWashBookings.Update(booking);
                await _context.SaveChangesAsync();

                return new CarWashServiceResponse<bool> { Success = true, Message = $"Booking status updated to {status}" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Car Wash booking status {BookingId}", bookingId);
                return new CarWashServiceResponse<bool> { Success = false, Message = "Error updating booking status" };
            }
        }

        // ─── Private helpers ──────────────────────────────────────────────────────

        private CarWashServiceDTOs MapToCarWashServiceDto(CarWashService service)
        {
            var includes = JsonSerializer.Deserialize<List<string>>(service.Includes) ?? new List<string>();
            return new CarWashServiceDTOs
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

        private CarWashBookingDTOs MapToCarWashBookingDto(CarWashBooking booking)
        {
            var serviceItems = JsonSerializer.Deserialize<List<CarWashBookingServiceItem>>(booking.SelectedServices)
                ?? new List<CarWashBookingServiceItem>();

            return new CarWashBookingDTOs
            {
                Id = booking.Id,
                BookingId = booking.BookingId,
                UserId = booking.UserId,
                CustomerPhone = booking.CustomerPhone,
                CustomerAddress = booking.CustomerAddress,
                VehicleSize = booking.VehicleSize,
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
        private async Task SaveToBookingHistoryAsync(CarWashBooking booking)
        {
            try
            {
                var legacy = new Booking
                {
                    BookingId = booking.BookingId,
                    UserId = booking.UserId,
                    ServiceId = 0,
                    SlotId = 0,
                    VehicleType = "Car",
                    ScheduledDate = booking.ScheduledDate,
                    ScheduledTime = booking.ScheduledTime,
                    Status = booking.Status,
                    Subtotal = booking.TotalAmount,
                    DiscountAmount = 0,
                    TaxAmount = 0,
                    TotalAmount = booking.TotalAmount,
                    PaymentStatus = booking.PaymentStatus,
                    SpecialInstructions = booking.SpecialInstructions,
                    CreatedAt = booking.CreatedAt
                };

                _context.Bookings.Add(legacy);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not mirror CarWashBooking {BookingId} to legacy Bookings table. This is non-fatal.", booking.BookingId);
                _context.ChangeTracker.Clear();
            }
        }
    }
}
