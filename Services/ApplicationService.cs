using CarWash.Api.Data;
using CarWash.Api.DTOs;
using CarWash.Api.Interfaces;
using CarWash.Api.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CarWash.Api.Entities;
using CarWash.Api.DTOs.CarWash.Api.DTOs;

namespace CarWash.Api.Services
{
    public class ApplicationService : IServiceService
    {
        private readonly AppDbContext _context;

        public ApplicationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<List<ServiceDto>>> GetAllServicesAsync(string category = null)
        {
            try
            {
                var query = _context.Services
                    .Include(s => s.Reviews)
                    .Where(s => s.IsActive);

                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(s => s.Category == category);
                }

                var services = await query
                    .OrderBy(s => s.DisplayOrder)
                    .ToListAsync();

                var serviceDtos = services.Select(MapToServiceDto).ToList();
                return ServiceResult<List<ServiceDto>>.SuccessResult(serviceDtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ServiceDto>>.FailureResult($"Failed to get services: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ServiceDto>> GetServiceByIdAsync(int id)
        {
            try
            {
                var service = await _context.Services
                    .Include(s => s.Reviews)
                    .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

                if (service == null)
                    return ServiceResult<ServiceDto>.FailureResult("Service not found");

                var serviceDto = MapToServiceDto(service);
                return ServiceResult<ServiceDto>.SuccessResult(serviceDto);
            }
            catch (Exception ex)
            {
                return ServiceResult<ServiceDto>.FailureResult($"Failed to get service: {ex.Message}");
            }
        }

        public async Task<ServiceResult<List<ServiceDto>>> GetPopularServicesAsync()
        {
            try
            {
                var services = await _context.Services
                    .Include(s => s.Reviews)
                    .Where(s => s.IsActive && s.IsPopular)
                    .OrderBy(s => s.DisplayOrder)
                    .Take(10)
                    .ToListAsync();

                var serviceDtos = services.Select(MapToServiceDto).ToList();
                return ServiceResult<List<ServiceDto>>.SuccessResult(serviceDtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ServiceDto>>.FailureResult($"Failed to get popular services: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ServiceAvailabilityDto>> CheckAvailabilityAsync(int serviceId, DateTime date)
        {
            try
            {
                var service = await _context.Services.FindAsync(serviceId);
                if (service == null || !service.IsActive)
                    return ServiceResult<ServiceAvailabilityDto>.FailureResult("Service not found");

                // Parse available slots
                var availableSlots = JsonSerializer.Deserialize<List<string>>(service.AvailableSlots ?? "[]");
                var unavailableDates = JsonSerializer.Deserialize<List<DateTime>>(service.UnavailableDates ?? "[]");

                // Check if date is unavailable
                if (unavailableDates.Any(d => d.Date == date.Date))
                {
                    return ServiceResult<ServiceAvailabilityDto>.SuccessResult(new ServiceAvailabilityDto
                    {
                        Date = date,
                        TimeSlots = new List<TimeSlotDto>()
                    }, "Date is unavailable");
                }

                // Get existing bookings for this date
                var existingBookings = await _context.Bookings
                    .Where(b => b.ServiceId == serviceId &&
                                b.ScheduledDate.Date == date.Date &&
                                b.Status != "cancelled")
                    .GroupBy(b => b.ScheduledTime)
                    .Select(g => new { Time = g.Key, Count = g.Count() })
                    .ToListAsync();

                var timeSlots = new List<TimeSlotDto>();

                foreach (var slot in availableSlots)
                {
                    var existingBooking = existingBookings.FirstOrDefault(b => b.Time == slot);
                    var bookedCount = existingBooking?.Count ?? 0;
                    var isAvailable = bookedCount < service.MaxBookingsPerSlot;

                    timeSlots.Add(new TimeSlotDto
                    {
                        Time = slot,
                        IsAvailable = isAvailable,
                        AvailableCount = service.MaxBookingsPerSlot - bookedCount
                    });
                }

                return ServiceResult<ServiceAvailabilityDto>.SuccessResult(new ServiceAvailabilityDto
                {
                    Date = date,
                    TimeSlots = timeSlots
                });
            }
            catch (Exception ex)
            {
                return ServiceResult<ServiceAvailabilityDto>.FailureResult($"Failed to check availability: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> IsTimeSlotAvailableAsync(int serviceId, DateTime date, string timeSlot)
        {
            try
            {
                var service = await _context.Services.FindAsync(serviceId);
                if (service == null || !service.IsActive)
                    return ServiceResult<bool>.FailureResult("Service not found");

                // Parse available slots
                var availableSlots = JsonSerializer.Deserialize<List<string>>(service.AvailableSlots ?? "[]");
                if (!availableSlots.Contains(timeSlot))
                    return ServiceResult<bool>.SuccessResult(false, "Time slot not offered");

                // Get existing bookings for this time slot
                var existingBookingsCount = await _context.Bookings
                    .CountAsync(b => b.ServiceId == serviceId &&
                                     b.ScheduledDate.Date == date.Date &&
                                     b.ScheduledTime == timeSlot &&
                                     b.Status != "cancelled");

                var isAvailable = existingBookingsCount < service.MaxBookingsPerSlot;
                return ServiceResult<bool>.SuccessResult(isAvailable);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult($"Failed to check time slot: {ex.Message}");
            }
        }

        // FIXED: Changed from WashingService to CarWash.Api.Entities.Service
        private ServiceDto MapToServiceDto(CarWash.Api.Entities.Service service)
        {
            var includes = JsonSerializer.Deserialize<List<string>>(service.Includes ?? "[]");
            var availableSlots = JsonSerializer.Deserialize<List<string>>(service.AvailableSlots ?? "[]");
            var unavailableDates = JsonSerializer.Deserialize<List<DateTime>>(service.UnavailableDates ?? "[]");

            var rating = service.Reviews?.Any() == true
                ? service.Reviews.Average(r => r.Rating)
                : 4.5; // Default rating

            var reviewCount = service.Reviews?.Count ?? 0;

            return new ServiceDto
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
                Category = service.Category,
                SubCategory = service.SubCategory,
                Price = service.Price,
                DiscountedPrice = service.DiscountedPrice,
                DurationInMinutes = service.DurationInMinutes,
                Includes = includes,
                ImageUrl = service.ImageUrl,
                IsPopular = service.IsPopular,
                Rating = rating,
                ReviewCount = reviewCount,
                AvailableSlots = availableSlots,
                UnavailableDates = unavailableDates
            };
        }
    }
}