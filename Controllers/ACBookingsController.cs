// Controllers/ACBookingsController.cs
using CarWash.Api.Models.DTOs.AC;
using CarWash.Api.Services.Interfaces.AC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;

namespace CarWash.Api.Controllers
{
    [ApiController]
    [Route("api/ac-bookings")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ACBookingsController : ControllerBase
    {
        private readonly IACServiceService _acServiceService;

        public ACBookingsController(IACServiceService acServiceService)
        {
            _acServiceService = acServiceService;
        }

        // ─── POST /api/ac-bookings ───────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ACBookingCreateDTOs bookingDto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _acServiceService.CreateBookingAsync(bookingDto, userId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ─── GET /api/ac-bookings/my-bookings ────────────────────────────────

        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _acServiceService.GetUserBookingsAsync(userId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ─── GET /api/ac-bookings/{id} ───────────────────────────────────────

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _acServiceService.GetBookingByIdAsync(id, userId.Value);
            return result.Success ? Ok(result) : NotFound(result);
        }

        // ─── POST /api/ac-bookings/{id}/cancel ──────────────────────────────

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _acServiceService.CancelBookingAsync(id, userId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ─── POST /api/ac-bookings/{id}/status (Admin/Technician) ───────────

        [HttpPost("{id:int}/status")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] ACUpdateStatusDto statusDto)
        {
            var result = await _acServiceService.UpdateBookingStatusAsync(id, statusDto.Status);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ─── Helper ───────────────────────────────────────────────────────────

        private Guid? GetUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            if (string.IsNullOrEmpty(claim)) return null;
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public class ACUpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}