using CarWash.Api.Data;
using CarWash.Api.Models.DTOs.Bookings;
using CarWash.Api.Models.DTOs.Slots;
using CarWash.Api.Models.Entities;
using CarWash.Api.Services.Interfaces.Slots;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarWash.Api.Services
{
    public class SlotService : ISlotService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SlotService> _logger;

        public SlotService(AppDbContext context, ILogger<SlotService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // === CRUD Operations ===
        public async Task<SlotDTOs> GetSlotByIdAsync(int id)
        {
            var slot = await _context.Slots
                .Include(s => s.Service)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (slot == null) return null;

            return MapToSlotDTO(slot);
        }

        public async Task<IEnumerable<SlotDTOs>> GetSlotsByDateAndServiceAsync(DateTime date, int serviceId)
        {
            var slots = await _context.Slots
                .Include(s => s.Service)
                .Where(s => s.Date.Date == date.Date && s.ServiceId == serviceId && s.IsActive)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return slots.Select(MapToSlotDTO);
        }

        public async Task<SlotDTOs> CreateSlotAsync(CreateSlotDTOs createSlotDto)
        {
            // Check for overlapping slots
            var existingSlot = await _context.Slots
                .FirstOrDefaultAsync(s =>
                    s.Date.Date == createSlotDto.Date.Date &&
                    s.ServiceId == createSlotDto.ServiceId &&
                    s.StartTime == createSlotDto.StartTime);

            if (existingSlot != null)
                throw new InvalidOperationException("A slot already exists for this time and service");

            var slot = new Slot
            {
                Date = createSlotDto.Date.Date,
                StartTime = createSlotDto.StartTime,
                EndTime = createSlotDto.EndTime,
                ServiceId = createSlotDto.ServiceId,
                MaxCapacity = createSlotDto.MaxCapacity,
                CurrentBookings = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Slots.Add(slot);
            await _context.SaveChangesAsync();

            // Reload with service info
            await _context.Entry(slot).Reference(s => s.Service).LoadAsync();
            return MapToSlotDTO(slot);
        }

        public async Task<SlotDTOs> UpdateSlotAsync(int id, CreateSlotDTOs updateSlotDto)
        {
            var slot = await _context.Slots.FindAsync(id);
            if (slot == null) return null;

            // Only allow updates if no bookings exist
            if (slot.CurrentBookings > 0)
                throw new InvalidOperationException("Cannot modify a slot with existing bookings");

            slot.StartTime = updateSlotDto.StartTime;
            slot.EndTime = updateSlotDto.EndTime;
            slot.MaxCapacity = updateSlotDto.MaxCapacity;
            slot.UpdatedAt = DateTime.UtcNow;

            _context.Slots.Update(slot);
            await _context.SaveChangesAsync();

            await _context.Entry(slot).Reference(s => s.Service).LoadAsync();
            return MapToSlotDTO(slot);
        }

        public async Task<bool> DeleteSlotAsync(int id)
        {
            var slot = await _context.Slots.FindAsync(id);
            if (slot == null) return false;

            // Only allow deletion if no bookings exist
            if (slot.CurrentBookings > 0)
                throw new InvalidOperationException("Cannot delete a slot with existing bookings");

            _context.Slots.Remove(slot);
            await _context.SaveChangesAsync();
            return true;
        }

        // === Availability Operations ===
        public async Task<SlotAvailabilityDTOs> GetSlotAvailabilityAsync(DateTime date, int serviceId)
        {
            var slots = await GetSlotsByDateAndServiceAsync(date, serviceId);
            var service = await _context.Services.FindAsync(serviceId);

            return new SlotAvailabilityDTOs
            {
                Date = date,
                TimeSlots = slots.Select(s => new TimeSlotDto
                {
                    SlotId = s.Id.ToString(), // FIX: Use slot ID, not time range
                    DisplayTime = FormatDisplayTime(s.StartTime),
                    AvailableSlots = s.AvailableSlots,
                    TotalCapacity = s.MaxCapacity,
                    IsAvailable = s.IsAvailable,
                    Status = s.AvailabilityStatus,
                    Color = GetStatusColor(s.AvailabilityStatus),
                    Price = service?.DiscountedPrice ?? service?.Price ?? 0
                }).ToList()
            };
        }

        public async Task<IEnumerable<SlotAvailabilityDTOs>> GetWeeklyAvailabilityAsync(DateTime startDate, int serviceId)
        {
            var results = new List<SlotAvailabilityDTOs>();
            for (int i = 0; i < 7; i++)
            {
                var date = startDate.AddDays(i);
                var availability = await GetSlotAvailabilityAsync(date, serviceId);
                results.Add(availability);
            }
            return results;
        }

        public async Task<bool> CheckSlotAvailabilityAsync(int slotId, int quantity = 1)
        {
            var slot = await _context.Slots.FindAsync(slotId);
            return slot != null
                && slot.IsActive
                && (slot.CurrentBookings + quantity) <= slot.MaxCapacity;
        }

        // === Booking Operations ===
        public async Task<BookingResultsDTOs> BookSlotAsync(BookingRequestDTOs request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Validate and lock the slot
                var slot = await _context.Slots
                    .FirstOrDefaultAsync(s => s.Id == request.SlotId && s.IsActive);

                if (slot == null)
                    return new BookingResultsDTOs
                    {
                        Success = false,
                        Message = "Slot not found"
                    };

                if (slot.CurrentBookings >= slot.MaxCapacity)
                    return new BookingResultsDTOs
                    {
                        Success = false,
                        Message = "Slot is full"
                    };

                // Update slot occupancy
                slot.CurrentBookings++;
                _context.Slots.Update(slot);

                // FIXED: Create booking with correct properties
                var booking = new Booking
                {
                    // Id is auto-generated by database
                    BookingId = GenerateBookingId(),
                    UserId = request.UserId,
                    ServiceId = request.ServiceId,
                    SlotId = request.SlotId,
                    AddressId = request.AddressId,
                    ScheduledDate = slot.Date, // Use ScheduledDate from slot
                    ScheduledTime = slot.StartTime, // Use ScheduledTime from slot
                    Status = "confirmed",
                    PaymentStatus = "pending",
                    VehicleType = request.VehicleType ?? "car",
                    Subtotal = request.Subtotal,
                    DiscountAmount = request.DiscountAmount ?? 0,
                    TaxAmount = request.TaxAmount ?? 0,
                    TotalAmount = request.TotalAmount,
                    SpecialInstructions = request.Notes, // Map Notes to SpecialInstructions
                    AppliedOfferCode = request.PromoCode, // Map PromoCode to AppliedOfferCode
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Bookings.AddAsync(booking);

                try
                {
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new BookingResultsDTOs
                    {
                        Success = true,
                        Message = "Booking successful",
                        Booking = booking,
                        BookingId = booking.Id, // This is the auto-generated INT Id
                        BookingReference = booking.BookingId, // This is the string BookingId
                        TotalAmount = booking.TotalAmount
                    };
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transaction.RollbackAsync();

                    // Check current availability
                    var currentSlot = await _context.Slots.FindAsync(slot.Id);
                    if (currentSlot.CurrentBookings >= currentSlot.MaxCapacity)
                        return new BookingResultsDTOs
                        {
                            Success = false,
                            Message = "Slot was just booked by another user"
                        };

                    return new BookingResultsDTOs
                    {
                        Success = false,
                        Message = "Please try again. Slot availability changed"
                    };
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Booking failed for slot {SlotId}", request.SlotId);
                return new BookingResultsDTOs
                {
                    Success = false,
                    Message = "Booking failed. Please try again."
                };
            }
        }

        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.Slot)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null) return false;

                // Only allow cancellation for pending or confirmed bookings
                if (booking.Status != "pending" && booking.Status != "confirmed")
                    return false;

                // Update slot occupancy
                if (booking.Slot != null && booking.Slot.CurrentBookings > 0)
                {
                    booking.Slot.CurrentBookings--;
                    _context.Slots.Update(booking.Slot);
                }

                // Update booking status
                booking.Status = "cancelled";
                booking.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        // === Bulk Operations ===
        public async Task<IEnumerable<SlotDTOs>> GenerateSlotsForServiceAsync(int serviceId, DateTime startDate, DateTime endDate)
        {
            var service = await _context.Services.FindAsync(serviceId);
            if (service == null)
                throw new InvalidOperationException("Service not found");

            var slots = new List<Slot>();
            var defaultSlots = new[]
            {
                new { Start = "09:00", End = "10:00", Capacity = service.MaxBookingsPerSlot },
                new { Start = "10:00", End = "11:00", Capacity = service.MaxBookingsPerSlot },
                new { Start = "11:00", End = "12:00", Capacity = service.MaxBookingsPerSlot },
                new { Start = "14:00", End = "15:00", Capacity = service.MaxBookingsPerSlot },
                new { Start = "15:00", End = "16:00", Capacity = service.MaxBookingsPerSlot },
                new { Start = "16:00", End = "17:00", Capacity = service.MaxBookingsPerSlot }
            };

            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                foreach (var timeSlot in defaultSlots)
                {
                    // Check if slot already exists
                    var exists = await _context.Slots
                        .AnyAsync(s => s.Date.Date == date.Date &&
                                     s.ServiceId == serviceId &&
                                     s.StartTime == timeSlot.Start);

                    if (!exists)
                    {
                        slots.Add(new Slot
                        {
                            Date = date,
                            StartTime = timeSlot.Start,
                            EndTime = timeSlot.End,
                            ServiceId = serviceId,
                            MaxCapacity = timeSlot.Capacity,
                            CurrentBookings = 0,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            if (slots.Any())
            {
                await _context.Slots.AddRangeAsync(slots);
                await _context.SaveChangesAsync();
            }

            return slots.Select(MapToSlotDTO);
        }

        // === Helper Methods ===
        private SlotDTOs MapToSlotDTO(Slot slot)
        {
            return new SlotDTOs
            {
                Id = slot.Id,
                Date = slot.Date,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                ServiceId = slot.ServiceId,
                ServiceName = slot.Service?.Name,
                Category = slot.Service?.Category,
                MaxCapacity = slot.MaxCapacity,
                CurrentBookings = slot.CurrentBookings,
                AvailableSlots = slot.AvailableSlots,
                IsAvailable = slot.IsAvailable,
                AvailabilityStatus = slot.AvailabilityStatus,
                ColorCode = GetStatusColor(slot.AvailabilityStatus),
                CreatedAt = slot.CreatedAt
            };
        }

        private string FormatDisplayTime(string time)
        {
            if (DateTime.TryParse(time, out var dt))
                return dt.ToString("hh:mm tt");
            return time;
        }

        private string GetStatusColor(string status)
        {
            return status switch
            {
                "Available" => "green",
                "Limited" => "yellow",
                "Full" => "red",
                _ => "gray"
            };
        }

        private string GenerateBookingId()
        {
            return $"BK{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
        }
    }
}