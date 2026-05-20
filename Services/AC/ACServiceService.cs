using CarWash.Api.Data;
using CarWash.Api.Models.DTOs;
using CarWash.Api.Models.DTOs.AC;
using CarWash.Api.Models.Entities;
using CarWash.Api.Models.Entities.AC;
using CarWash.Api.Services.Interfaces;
using CarWash.Api.Services.Interfaces.AC;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CarWash.Api.Services.AC
{

    public class ACServiceService : IACServiceService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ACServiceService> _logger;

        public ACServiceService(AppDbContext context, ILogger<ACServiceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResponse<List<ACServiceDTOs>>> GetAllServicesAsync(ACServiceFilterDTOs? filter = null)
        {
            try
            {
                var query = _context.ACServices.Where(s => s.IsActive);

                if (!string.IsNullOrEmpty(filter?.Category))
                {
                    query = query.Where(s => s.Category == filter.Category);
                }

                if (filter?.IsPopular.HasValue == true)
                {
                    query = query.Where(s => s.IsPopular == filter.IsPopular);
                }

                var services = await query
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Name)
                    .ToListAsync();

                var serviceDtos = services.Select(MapToACServiceDto).ToList();

                return new ServiceResponse<List<ACServiceDTOs>>
                {
                    Success = true,
                    Data = serviceDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AC services");
                return new ServiceResponse<List<ACServiceDTOs>>
                {
                    Success = false,
                    Message = "Error retrieving services"
                };
            }
        }

        public async Task<ServiceResponse<ACServiceDTOs>> GetServiceByIdAsync(int id)
        {
            try
            {
                var service = await _context.ACServices.FindAsync(id);
                if (service == null || !service.IsActive)
                {
                    return new ServiceResponse<ACServiceDTOs>
                    {
                        Success = false,
                        Message = "Service not found"
                    };
                }

                return new ServiceResponse<ACServiceDTOs>
                {
                    Success = true,
                    Data = MapToACServiceDto(service)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AC service by ID: {Id}", id);
                return new ServiceResponse<ACServiceDTOs>
                {
                    Success = false,
                    Message = "Error retrieving service"
                };
            }
        }

        public async Task<ServiceResponse<List<ACServiceDTOs>>> GetServicesByCategoryAsync(string category)
        {
            try
            {
                var services = await _context.ACServices
                    .Where(s => s.IsActive && s.Category == category)
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Name)
                    .ToListAsync();

                var serviceDtos = services.Select(MapToACServiceDto).ToList();

                return new ServiceResponse<List<ACServiceDTOs>>
                {
                    Success = true,
                    Data = serviceDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AC services by category: {Category}", category);
                return new ServiceResponse<List<ACServiceDTOs>>
                {
                    Success = false,
                    Message = "Error retrieving services"
                };
            }
        }

        public async Task<ServiceResponse<List<ACServiceDTOs>>> GetPopularServicesAsync()
        {
            try
            {
                var services = await _context.ACServices
                    .Where(s => s.IsActive && s.IsPopular)
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Name)
                    .Take(10)
                    .ToListAsync();

                var serviceDtos = services.Select(MapToACServiceDto).ToList();

                return new ServiceResponse<List<ACServiceDTOs>>
                {
                    Success = true,
                    Data = serviceDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular AC services");
                return new ServiceResponse<List<ACServiceDTOs>>
                {
                    Success = false,
                    Message = "Error retrieving services"
                };
            }
        }

        public async Task<ServiceResponse<ACBookingResponseDTOs>> CreateBookingAsync(ACBookingCreateDTOs bookingDto, Guid userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get services
                var services = await _context.ACServices
                    .Where(s => bookingDto.ServiceIds.Contains(s.Id) && s.IsActive)
                    .ToListAsync();

                if (services.Count != bookingDto.ServiceIds.Count)
                {
                    return new ServiceResponse<ACBookingResponseDTOs>
                    {
                        Success = false,
                        Message = "One or more services not found"
                    };
                }

                // Calculate total amount
                var totalAmount = services.Sum(s => s.Price);

                // Create booking
                var booking = new ACBooking
                {
                    BookingId = GenerateACBookingId(),
                    UserId = userId,
                    CustomerPhone = bookingDto.CustomerPhone,
                    CustomerAddress = bookingDto.CustomerAddress,
                    ACType = bookingDto.ACType,
                    ACBrand = bookingDto.ACBrand ?? string.Empty,
                    ACCapacity = bookingDto.ACCapacity ?? string.Empty,
                    UsageType = bookingDto.UsageType ?? string.Empty,
                    ScheduledDate = bookingDto.ScheduledDate.Date,
                    ScheduledTime = bookingDto.ScheduledTime,
                    TotalAmount = totalAmount,
                    SpecialInstructions = bookingDto.SpecialInstructions ?? string.Empty,
                    Status = "confirmed",
                    PaymentStatus = "pending",
                    CreatedAt = DateTime.UtcNow
                };

                // Create service items
                var serviceItems = services.Select(s => new ACBookingServiceItem
                {
                    ServiceId = s.Id,
                    ServiceName = s.Name,
                    Price = s.Price,
                    Duration = s.DurationDisplay,
                    Includes = s.Includes
                }).ToList();

                booking.SelectedServices = JsonSerializer.Serialize(serviceItems);
                booking.ServiceItems = serviceItems;

                _context.ACBookings.Add(booking);
                await _context.SaveChangesAsync();

                // Save to history as well (for compatibility with existing app)
                await SaveToBookingHistoryAsync(booking);

                await transaction.CommitAsync();

                var bookingDtoResult = MapToACBookingDto(booking);

                return new ServiceResponse<ACBookingResponseDTOs>
                {
                    Success = true,
                    Message = "Booking created successfully",
                    Data = new ACBookingResponseDTOs
                    {
                        Booking = bookingDtoResult,
                        BookingId = booking.BookingId,
                        TotalAmount = totalAmount
                    }
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating AC booking");
                return new ServiceResponse<ACBookingResponseDTOs>
                {
                    Success = false,
                    Message = "Error creating booking"
                };
            }
        }

        public async Task<ServiceResponse<List<ACBookingDTOs>>> GetUserBookingsAsync(Guid userId)
        {
            try
            {
                var bookings = await _context.ACBookings
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                var bookingDtos = bookings.Select(MapToACBookingDto).ToList();

                return new ServiceResponse<List<ACBookingDTOs>>
                {
                    Success = true,
                    Data = bookingDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user AC bookings for user: {UserId}", userId);
                return new ServiceResponse<List<ACBookingDTOs>>
                {
                    Success = false,
                    Message = "Error retrieving bookings"
                };
            }
        }

        public async Task<ServiceResponse<ACBookingDTOs>> GetBookingByIdAsync(int bookingId, Guid userId)
        {
            try
            {
                var booking = await _context.ACBookings
                    .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

                if (booking == null)
                {
                    return new ServiceResponse<ACBookingDTOs>
                    {
                        Success = false,
                        Message = "Booking not found"
                    };
                }

                return new ServiceResponse<ACBookingDTOs>
                {
                    Success = true,
                    Data = MapToACBookingDto(booking)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AC booking by ID: {BookingId}", bookingId);
                return new ServiceResponse<ACBookingDTOs>
                {
                    Success = false,
                    Message = "Error retrieving booking"
                };
            }
        }

        public async Task<ServiceResponse<bool>> CancelBookingAsync(int bookingId, Guid userId)
        {
            try
            {
                var booking = await _context.ACBookings
                    .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

                if (booking == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "Booking not found"
                    };
                }

                if (booking.Status == "completed" || booking.Status == "cancelled")
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Booking cannot be cancelled. Current status: {booking.Status}"
                    };
                }

                booking.Status = "cancelled";
                booking.UpdatedAt = DateTime.UtcNow;

                _context.ACBookings.Update(booking);
                await _context.SaveChangesAsync();

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Message = "Booking cancelled successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling AC booking: {BookingId}", bookingId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Error cancelling booking"
                };
            }
        }

        public async Task<ServiceResponse<bool>> UpdateBookingStatusAsync(int bookingId, string status)
        {
            try
            {
                var booking = await _context.ACBookings.FindAsync(bookingId);
                if (booking == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "Booking not found"
                    };
                }

                booking.Status = status;
                booking.UpdatedAt = DateTime.UtcNow;

                if (status == "completed")
                {
                    booking.CompletedAt = DateTime.UtcNow;
                }

                _context.ACBookings.Update(booking);
                await _context.SaveChangesAsync();

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Message = $"Booking status updated to {status}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating AC booking status: {BookingId}", bookingId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Error updating booking status"
                };
            }
        }

        // Helper methods
        private ACServiceDTOs MapToACServiceDto(ACService service)
        {
            var includes = JsonSerializer.Deserialize<List<string>>(service.Includes) ?? new List<string>();

            return new ACServiceDTOs
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

        private ACBookingDTOs MapToACBookingDto(ACBooking booking)
        {
            var serviceItems = JsonSerializer.Deserialize<List<ACBookingServiceItem>>(booking.SelectedServices)
                ?? new List<ACBookingServiceItem>();

            return new ACBookingDTOs
            {
                Id = booking.Id,
                BookingId = booking.BookingId,
                UserId = booking.UserId,
                CustomerPhone = booking.CustomerPhone,
                CustomerAddress = booking.CustomerAddress,
                ACType = booking.ACType,
                ACBrand = booking.ACBrand,
                ACCapacity = booking.ACCapacity,
                UsageType = booking.UsageType,
                ScheduledDate = booking.ScheduledDate,
                ScheduledTime = booking.ScheduledTime,
                Status = booking.Status,
                TotalAmount = booking.TotalAmount,
                PaymentStatus = booking.PaymentStatus,
                CreatedAt = booking.CreatedAt,
                ServiceItems = serviceItems
            };
        }

        private string GenerateACBookingId()
        {
            var date = DateTime.UtcNow.ToString("yyMMdd");
            var random = new Random().Next(1000, 9999);
            return $"AC{date}{random}";
        }

        private async Task SaveToBookingHistoryAsync(ACBooking booking)
        {
            try
            {
                // Also save to existing booking table for compatibility
                var existingBooking = new Booking
                {
                    BookingId = booking.BookingId,
                    UserId = booking.UserId,
                    ServiceId = 0, // Not applicable for AC services
                    SlotId = 0,
                    VehicleType = "AC",
                    ACType = booking.ACType,
                    ACBrand = booking.ACBrand,
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

                _context.Bookings.Add(existingBooking);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving AC booking to history");
                // Don't throw, just log
            }
        }
    }
}


