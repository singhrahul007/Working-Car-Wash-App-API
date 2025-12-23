using CarWash.Api.Models.DTOs.Bookings;
using CarWash.Api.Models.DTOs.Slots;
using CarWash.Api.Services.Interfaces.Slots;
using Microsoft.AspNetCore.Mvc;
//using Swashbuckle.AspNetCore.Annotations;

namespace CarWash.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class SlotsController : ControllerBase
    {
        private readonly ISlotService _slotService;

        public SlotsController(ISlotService slotService)
        {
            _slotService = slotService;
        }

        /// <summary>
        /// Get a specific slot by ID
        /// </summary>
        /// <param name="id">Slot ID</param>
        /// <returns>Slot details</returns>
        [HttpGet("{id}")]
        //[SwaggerOperation(Summary = "Get slot by ID", Description = "Retrieves a specific slot by its ID")]
        //[SwaggerResponse(200, "Slot found", typeof(SlotDTOs))]
        //[SwaggerResponse(404, "Slot not found")]
        public async Task<ActionResult<SlotDTOs>> GetSlot(int id)
        {
            var slot = await _slotService.GetSlotByIdAsync(id);
            if (slot == null)
                return NotFound(new { message = "Slot not found" });

            return Ok(slot);
        }

        /// <summary>
        /// Get available slots for a specific date and service
        /// </summary>
        /// <param name="date">Date to check (format: yyyy-MM-dd)</param>
        /// <param name="serviceId">Service ID</param>
        /// <returns>List of available slots with pricing</returns>
        [HttpGet("availability")]
        //[SwaggerOperation(Summary = "Get slot availability", Description = "Get available time slots for a specific date and service")]
        //[SwaggerResponse(200, "Availability retrieved", typeof(SlotAvailabilityDTOs))]
        public async Task<ActionResult<SlotAvailabilityDTOs>> GetSlotAvailability(
            [FromQuery] DateTime date,
            [FromQuery] int serviceId)
        {
            var availability = await _slotService.GetSlotAvailabilityAsync(date, serviceId);
            return Ok(availability);
        }

        /// <summary>
        /// Get weekly availability for a service
        /// </summary>
        /// <param name="startDate">Start date (format: yyyy-MM-dd)</param>
        /// <param name="serviceId">Service ID</param>
        /// <returns>Weekly availability from start date</returns>
        [HttpGet("weekly-availability")]
        //[SwaggerOperation(Summary = "Get weekly availability", Description = "Get availability for 7 days starting from specified date")]
        //[SwaggerResponse(200, "Weekly availability retrieved", typeof(IEnumerable<SlotAvailabilityDTOs>))]
        public async Task<ActionResult<IEnumerable<SlotAvailabilityDTOs>>> GetWeeklyAvailability(
            [FromQuery] DateTime startDate,
            [FromQuery] int serviceId)
        {
            var availability = await _slotService.GetWeeklyAvailabilityAsync(startDate, serviceId);
            return Ok(availability);
        }

        /// <summary>
        /// Get all slots for a specific date and service
        /// </summary>
        /// <param name="date">Date to check (format: yyyy-MM-dd)</param>
        /// <param name="serviceId">Service ID</param>
        /// <returns>List of all slots for the date</returns>
        [HttpGet("by-date")]
        //[SwaggerOperation(Summary = "Get slots by date", Description = "Get all slots for a specific date and service")]
        //[SwaggerResponse(200, "Slots retrieved", typeof(IEnumerable<SlotDTOs>))]
        public async Task<ActionResult<IEnumerable<SlotDTOs>>> GetSlotsByDate(
            [FromQuery] DateTime date,
            [FromQuery] int serviceId)
        {
            var slots = await _slotService.GetSlotsByDateAndServiceAsync(date, serviceId);
            return Ok(slots);
        }

        /// <summary>
        /// Create a new time slot
        /// </summary>
        /// <param name="createSlotDto">Slot creation data</param>
        /// <returns>Created slot details</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/slots
        ///     {
        ///         "date": "2024-01-20T00:00:00",
        ///         "startTime": "09:00",
        ///         "endTime": "10:00",
        ///         "serviceId": 1,
        ///         "maxCapacity": 5
        ///     }
        /// </remarks>
        [HttpPost]
        //[SwaggerOperation(Summary = "Create a new slot", Description = "Creates a new time slot for a service")]
        //[SwaggerResponse(201, "Slot created successfully", typeof(SlotDTOs))]
        //[SwaggerResponse(400, "Invalid data or slot already exists")]
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

        /// <summary>
        /// Generate multiple slots for a service
        /// </summary>
        /// <param name="request">Generation parameters</param>
        /// <returns>List of generated slots</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/slots/generate
        ///     {
        ///         "serviceId": 1,
        ///         "startDate": "2024-01-20",
        ///         "endDate": "2024-01-25"
        ///     }
        ///     
        /// This will generate default time slots (09:00-17:00) for each day in the range.
        /// </remarks>
        [HttpPost("generate")]
        //[SwaggerOperation(Summary = "Generate slots", Description = "Generate multiple time slots for a service over a date range")]
        //[SwaggerResponse(200, "Slots generated", typeof(IEnumerable<SlotDTOs>))]
        public async Task<ActionResult<IEnumerable<SlotDTOs>>> GenerateSlots(
            [FromBody] SlotGenerationRequestDTOs request)
        {
            var slots = await _slotService.GenerateSlotsForServiceAsync(
                request.ServiceId,
                request.StartDate,
                request.EndDate);

            return Ok(slots);
        }

        /// <summary>
        /// Update an existing slot
        /// </summary>
        /// <param name="id">Slot ID to update</param>
        /// <param name="updateSlotDto">Updated slot data</param>
        /// <returns>Updated slot details</returns>
        [HttpPut("{id}")]
        //[SwaggerOperation(Summary = "Update slot", Description = "Update an existing time slot")]
        //[SwaggerResponse(200, "Slot updated", typeof(SlotDTOs))]
        //[SwaggerResponse(404, "Slot not found")]
        //[SwaggerResponse(400, "Cannot update slot with existing bookings")]
        public async Task<ActionResult<SlotDTOs>> UpdateSlot(
            int id,
            [FromBody] CreateSlotDTOs updateSlotDto)
        {
            var slot = await _slotService.UpdateSlotAsync(id, updateSlotDto);
            if (slot == null)
                return NotFound(new { message = "Slot not found" });

            return Ok(slot);
        }

        /// <summary>
        /// Delete a slot
        /// </summary>
        /// <param name="id">Slot ID to delete</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        //[SwaggerOperation(Summary = "Delete slot", Description = "Delete a time slot")]
        //[SwaggerResponse(204, "Slot deleted")]
        //[SwaggerResponse(404, "Slot not found")]
        //[SwaggerResponse(400, "Cannot delete slot with existing bookings")]
        public async Task<IActionResult> DeleteSlot(int id)
        {
            var result = await _slotService.DeleteSlotAsync(id);
            if (!result)
                return NotFound(new { message = "Slot not found" });

            return NoContent();
        }

        /// <summary>
        /// Check if a slot is available for booking
        /// </summary>
        /// <param name="request">Availability check request</param>
        /// <returns>Availability status</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/slots/check-availability
        ///     {
        ///         "slotId": 1,
        ///         "quantity": 1
        ///     }
        /// </remarks>
        [HttpPost("check-availability")]
        //[SwaggerOperation(Summary = "Check slot availability", Description = "Check if a specific slot has available capacity")]
        //[SwaggerResponse(200, "Availability checked", typeof(SlotAvailabilityResponseDTOs))]
        public async Task<ActionResult<SlotAvailabilityResponseDTOs>> CheckAvailability(
            [FromBody] SlotAvailabilityRequestDTOs request)
        {
            var isAvailable = await _slotService.CheckSlotAvailabilityAsync(
                request.SlotId,
                request.Quantity);

            return Ok(new SlotAvailabilityResponseDTOs
            {
                IsAvailable = isAvailable,
                Message = isAvailable ? "Slot is available" : "Slot is not available"
            });
        }

        /// <summary>
        /// Book a time slot
        /// </summary>
        /// <param name="bookingRequest">Booking details</param>
        /// <returns>Booking confirmation</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/slots/book
        ///     {
        ///         "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///         "serviceId": 1,
        ///         "slotId": 1,
        ///         "addressId": 1,
        ///         "subtotal": 299.99,
        ///         "totalAmount": 299.99,
        ///         "vehicleType": "car",
        ///         "notes": "Please wash thoroughly",
        ///         "promoCode": "WELCOME10"
        ///     }
        /// </remarks>
        [HttpPost("book")]
        //[SwaggerOperation(Summary = "Book a slot", Description = "Book a specific time slot")]
        //[SwaggerResponse(200, "Booking successful", typeof(BookingResultsDTOs))]
        //[SwaggerResponse(400, "Booking failed")]
        public async Task<ActionResult<BookingResultsDTOs>> BookSlot([FromBody] BookingRequestDTOs bookingRequest)
        {
            var result = await _slotService.BookSlotAsync(bookingRequest);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(result);
        }

        /// <summary>
        /// Cancel a booking
        /// </summary>
        /// <param name="bookingId">Booking ID to cancel</param>
        /// <returns>Cancellation confirmation</returns>
        [HttpPost("cancel-booking/{bookingId}")]
        //[SwaggerOperation(Summary = "Cancel booking", Description = "Cancel an existing booking and free up the slot")]
        //[SwaggerResponse(200, "Booking cancelled")]
        //[SwaggerResponse(404, "Booking not found")]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var result = await _slotService.CancelBookingAsync(bookingId);
            if (!result)
                return NotFound(new { message = "Booking not found" });

            return Ok(new { message = "Booking cancelled successfully" });
        }
    }

    
}