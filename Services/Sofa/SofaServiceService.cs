using CarWash.Api.Data;
using CarWash.Api.Models.Entities;
using CarWash.Api.Models.Entities.Sofa;
using CarWash.Api.Models.DTOs.Sofa;
using CarWash.Api.Services.Interfaces.Sofa;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CarWash.Api.Services.Sofa
{

    public class SofaServiceService : ISofaServiceService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SofaServiceService> _logger;

        public SofaServiceService(AppDbContext context, ILogger<SofaServiceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ─── Services ────────────────────────────────────────────────────────────

        public async Task<SofaServiceResponse<List<SofaServiceDTOs>>> GetAllServicesAsync(SofaServiceFilterDTOs? filter = null)
        {
            try
            {
                var query = _context.SofaServices.Where(s => s.IsActive);

                if (!string.IsNullOrEmpty(filter?.Category))
                    query = query.Where(s => s.Category == filter.Category);

                if (filter?.IsPopular.HasValue == true)
                    query = query.Where(s => s.IsPopular == filter.IsPopular);

                var services = await query
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Name)
                    .ToListAsync();

                return new SofaServiceResponse<List<SofaServiceDTOs>>
                {
                    Success = true,
                    Data = services.Select(MapToSofaServiceDto).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Sofa services");
                return new SofaServiceResponse<List<SofaServiceDTOs>> { Success = false, Message = "Error retrieving services" };
            }
        }

        public async Task<SofaServiceResponse<SofaServiceDTOs>> GetServiceByIdAsync(int id)
        {
            try
            {
                var service = await _context.SofaServices.FindAsync(id);
                if (service == null || !service.IsActive)
                    return new SofaServiceResponse<SofaServiceDTOs> { Success = false, Message = "Service not found" };

                return new SofaServiceResponse<SofaServiceDTOs> { Success = true, Data = MapToSofaServiceDto(service) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Sofa service by ID: {Id}", id);
                return new SofaServiceResponse<SofaServiceDTOs> { Success = false, Message = "Error retrieving service" };
            }
        }

        public async Task<SofaServiceResponse<List<SofaServiceDTOs>>> GetServicesByCategoryAsync(string category)
        {
            try
            {
                var services = await _context.SofaServices
                    .Where(s => s.IsActive && s.Category == category)
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Name)
                    .ToListAsync();

                return new SofaServiceResponse<List<SofaServiceDTOs>>
                {
                    Success = true,
                    Data = services.Select(MapToSofaServiceDto).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Sofa services by category: {Category}", category);
                return new SofaServiceResponse<List<SofaServiceDTOs>> { Success = false, Message = "Error retrieving services" };
            }
        }

        public async Task<SofaServiceResponse<List<SofaServiceDTOs>>> GetPopularServicesAsync()
        {
            try
            {
                var services = await _context.SofaServices
                    .Where(s => s.IsActive && s.IsPopular)
                    .OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Name)
                    .Take(10)
                    .ToListAsync();

                return new SofaServiceResponse<List<SofaServiceDTOs>>
                {
                    Success = true,
                    Data = services.Select(MapToSofaServiceDto).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular Sofa services");
                return new SofaServiceResponse<List<SofaServiceDTOs>> { Success = false, Message = "Error retrieving services" };
            }
        }

        // ─── Bookings ─────────────────────────────────────────────────────────────

        public async Task<SofaServiceResponse<SofaBookingResponseDTOs>> CreateBookingAsync(SofaBookingCreateDTOs bookingDto, Guid userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate service IDs
                var services = await _context.SofaServices
                    .Where(s => bookingDto.ServiceIds.Contains(s.Id) && s.IsActive)
                    .ToListAsync();

                if (services.Count != bookingDto.ServiceIds.Count)
                    return new SofaServiceResponse<SofaBookingResponseDTOs>
                    {
                        Success = false,
                        Message = "One or more services not found or inactive"
                    };

                // Calculate total (price × sofa count)
                var baseAmount = services.Sum(s => s.Price);
                var totalAmount = baseAmount * bookingDto.SofaCount;

                // Build service items for JSON storage
                var serviceItems = services.Select(s => new SofaBookingServiceItem
                {
                    ServiceId = s.Id,
                    ServiceName = s.Name,
                    Price = s.Price,
                    Duration = s.DurationDisplay,
                    Includes = s.Includes
                }).ToList();

                // Create the booking
                var booking = new SofaBooking
                {
                    BookingId = GenerateSofaBookingId(),
                    UserId = userId,
                    CustomerPhone = bookingDto.CustomerPhone,
                    CustomerAddress = bookingDto.CustomerAddress,
                    SofaType = bookingDto.SofaType,
                    SofaCount = bookingDto.SofaCount,
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

                _context.SofaBookings.Add(booking);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new SofaServiceResponse<SofaBookingResponseDTOs>
                {
                    Success = true,
                    Message = "Sofa cleaning booking created successfully",
                    Data = new SofaBookingResponseDTOs
                    {
                        Booking = MapToSofaBookingDto(booking),
                        BookingId = booking.BookingId,
                        TotalAmount = totalAmount
                    }
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating Sofa booking for user {UserId}", userId);
                return new SofaServiceResponse<SofaBookingResponseDTOs> { Success = false, Message = "Error creating booking. Please try again." };
            }
        }

        public async Task<SofaServiceResponse<List<SofaBookingDTOs>>> GetUserBookingsAsync(Guid userId)
        {
            try
            {
                var bookings = await _context.SofaBookings
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                return new SofaServiceResponse<List<SofaBookingDTOs>>
                {
                    Success = true,
                    Data = bookings.Select(MapToSofaBookingDto).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Sofa bookings for user {UserId}", userId);
                return new SofaServiceResponse<List<SofaBookingDTOs>> { Success = false, Message = "Error retrieving bookings" };
            }
        }

        public async Task<SofaServiceResponse<SofaBookingDTOs>> GetBookingByIdAsync(int bookingId, Guid userId)
        {
            try
            {
                var booking = await _context.SofaBookings
                    .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

                if (booking == null)
                    return new SofaServiceResponse<SofaBookingDTOs> { Success = false, Message = "Booking not found" };

                return new SofaServiceResponse<SofaBookingDTOs> { Success = true, Data = MapToSofaBookingDto(booking) };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Sofa booking {BookingId}", bookingId);
                return new SofaServiceResponse<SofaBookingDTOs> { Success = false, Message = "Error retrieving booking" };
            }
        }

        public async Task<SofaServiceResponse<bool>> CancelBookingAsync(int bookingId, Guid userId)
        {
            try
            {
                var booking = await _context.SofaBookings
                    .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

                if (booking == null)
                    return new SofaServiceResponse<bool> { Success = false, Message = "Booking not found" };

                if (booking.Status is "completed" or "cancelled")
                    return new SofaServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Booking cannot be cancelled. Current status: {booking.Status}"
                    };

                booking.Status = "cancelled";
                booking.UpdatedAt = DateTime.UtcNow;
                _context.SofaBookings.Update(booking);
                await _context.SaveChangesAsync();

                return new SofaServiceResponse<bool> { Success = true, Message = "Booking cancelled successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling Sofa booking {BookingId}", bookingId);
                return new SofaServiceResponse<bool> { Success = false, Message = "Error cancelling booking" };
            }
        }

        public async Task<SofaServiceResponse<bool>> UpdateBookingStatusAsync(int bookingId, string status)
        {
            try
            {
                var booking = await _context.SofaBookings.FindAsync(bookingId);
                if (booking == null)
                    return new SofaServiceResponse<bool> { Success = false, Message = "Booking not found" };

                booking.Status = status;
                booking.UpdatedAt = DateTime.UtcNow;
                if (status == "completed")
                    booking.CompletedAt = DateTime.UtcNow;

                _context.SofaBookings.Update(booking);
                await _context.SaveChangesAsync();

                return new SofaServiceResponse<bool> { Success = true, Message = $"Booking status updated to {status}" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Sofa booking status {BookingId}", bookingId);
                return new SofaServiceResponse<bool> { Success = false, Message = "Error updating booking status" };
            }
        }

        // ─── Private helpers ──────────────────────────────────────────────────────

        private SofaServiceDTOs MapToSofaServiceDto(SofaService service)
        {
            var includes = JsonSerializer.Deserialize<List<string>>(service.Includes) ?? new List<string>();
            return new SofaServiceDTOs
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

        private SofaBookingDTOs MapToSofaBookingDto(SofaBooking booking)
        {
            var serviceItems = JsonSerializer.Deserialize<List<SofaBookingServiceItem>>(booking.SelectedServices)
                ?? new List<SofaBookingServiceItem>();

            return new SofaBookingDTOs
            {
                Id = booking.Id,
                BookingId = booking.BookingId,
                UserId = booking.UserId,
                CustomerPhone = booking.CustomerPhone,
                CustomerAddress = booking.CustomerAddress,
                SofaType = booking.SofaType,
                SofaCount = booking.SofaCount,
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

        private static string GenerateSofaBookingId()
        {
            var date = DateTime.UtcNow.ToString("yyMMdd");
            var random = new Random().Next(1000, 9999);
            return $"SOFA{date}{random}";
        }
    }
}
