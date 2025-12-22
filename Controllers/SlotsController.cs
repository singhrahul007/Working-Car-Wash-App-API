using CarWash.Api.Models.DTOs.Bookings;
using CarWash.Api.Models.DTOs.Slots;
using CarWash.Api.Services.Interfaces.Slots;
using Microsoft.AspNetCore.Mvc;

namespace CarWash.Api.Controllers
{
    public class SlotsController : Controller
    {
        private readonly ISlotService _slotService;

        public SlotsController(ISlotService slotService)
        {
            _slotService = slotService;
        }

        // GET: api/slots/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SlotDTOs>> GetSlot(int id)
        {
            var slot = await _slotService.GetSlotByIdAsync(id);
            if (slot == null)
                return NotFound(new { message = "Slot not found" });

            return Ok(slot);
        }
        // GET: api/slots/availability?date=2024-01-15&serviceId=1
        [HttpGet("availability")]
        public async Task<ActionResult<SlotAvailabilityDTOs>> GetSlotAvailability(
            [FromQuery] DateTime date,
            [FromQuery] int serviceId)
        {
            var availability = await _slotService.GetSlotAvailabilityAsync(date, serviceId);
            return Ok(availability);
        }
        // GET: api/slots/weekly-availability?startDate=2024-01-15&serviceId=1
        [HttpGet("weekly-availability")]
        public async Task<ActionResult<IEnumerable<SlotAvailabilityDTOs>>> GetWeeklyAvailability(
            [FromQuery] DateTime startDate,
            [FromQuery] int serviceId)
        {
            var availability = await _slotService.GetWeeklyAvailabilityAsync(startDate, serviceId);
            return Ok(availability);
        }
        // GET: api/slots/by-date?date=2024-01-15&serviceId=1
        [HttpGet("by-date")]
        public async Task<ActionResult<IEnumerable<SlotDTOs>>> GetSlotsByDate(
            [FromQuery] DateTime date,
            [FromQuery] int serviceId)
        {
            var slots = await _slotService.GetSlotsByDateAndServiceAsync(date, serviceId);
            return Ok(slots);
        }
        // POST: api/slots
        [HttpPost]
        public async Task<ActionResult<SlotDTOs>> CreateSlot([FromBody] CreateSlotDTOs createSlotDto)
        {
            try
            {
                var slot = await _slotService.CreateSlotAsync(createSlotDto);
                return CreatedAtAction(nameof(GetSlot), new { id = slot.Id }, slot);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        // POST: api/slots/generate
        [HttpPost("generate")]
        public async Task<ActionResult<IEnumerable<SlotDTOs>>> GenerateSlots(
            [FromBody] GenerateSlotsRequest request)
        {
            var slots = await _slotService.GenerateSlotsForServiceAsync(
                request.ServiceId,
                request.StartDate,
                request.EndDate);

            return Ok(slots);
        }
        // PUT: api/slots/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<SlotDTOs>> UpdateSlot(
            int id,
            [FromBody] CreateSlotDTOs updateSlotDto)
        {
            var slot = await _slotService.UpdateSlotAsync(id, updateSlotDto);
            if (slot == null)
                return NotFound(new { message = "Slot not found" });

            return Ok(slot);
        }
        // DELETE: api/slots/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSlot(int id)
        {
            var result = await _slotService.DeleteSlotAsync(id);
            if (!result)
                return NotFound(new { message = "Slot not found" });

            return NoContent();
        }
        // POST: api/slots/check-availability
        [HttpPost("check-availability")]
        public async Task<ActionResult<AvailabilityCheckResult>> CheckAvailability(
            [FromBody] AvailabilityCheckRequest request)
        {
            var isAvailable = await _slotService.CheckSlotAvailabilityAsync(
                request.SlotId,
                request.Quantity);

            return Ok(new AvailabilityCheckResult
            {
                IsAvailable = isAvailable,
                Message = isAvailable ? "Slot is available" : "Slot is not available"
            });
        }
        // POST: api/slots/book
        [HttpPost("book")]
        public async Task<ActionResult<BookingResultsDTOs>> BookSlot([FromBody] BookingRequestDTOs bookingRequest)
        {
            var result = await _slotService.BookSlotAsync(bookingRequest);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(result);
        }

        // POST: api/slots/cancel-booking/{bookingId}
        [HttpPost("cancel-booking/{bookingId}")]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var result = await _slotService.CancelBookingAsync(bookingId);
            if (!result)
                return NotFound(new { message = "Booking not found" });

            return Ok(new { message = "Booking cancelled successfully" });
        }
    }

    // Additional DTOs for controller
    public class GenerateSlotsRequest
    {
        public int ServiceId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class AvailabilityCheckRequest
    {
        public int SlotId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class AvailabilityCheckResult
    {
        public bool IsAvailable { get; set; }
        public string Message { get; set; }
    }

}

