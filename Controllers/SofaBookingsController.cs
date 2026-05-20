// Controllers/SofaBookingsController.cs
using CarWash.Api.Models.DTOs.Sofa;
using CarWash.Api.Services.Interfaces.Sofa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarWash.Api.Controllers
{
    [ApiController]
    [Route("api/sofa-bookings")]
    [Authorize(AuthenticationSchemes = "JwtBearer")]
    public class SofaBookingsController : ControllerBase
    {
        private readonly ISofaServiceService _sofaService;

        public SofaBookingsController(ISofaServiceService sofaService)
        {
            _sofaService = sofaService;
        }

        // ─── POST /api/sofa-bookings ───────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SofaBookingCreateDTOs bookingDto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _sofaService.CreateBookingAsync(bookingDto, userId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ─── GET /api/sofa-bookings/my-bookings ───────────────────────────────

        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _sofaService.GetUserBookingsAsync(userId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ─── GET /api/sofa-bookings/{id} ──────────────────────────────────────

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _sofaService.GetBookingByIdAsync(id, userId.Value);
            return result.Success ? Ok(result) : NotFound(result);
        }

        // ─── POST /api/sofa-bookings/{id}/cancel ──────────────────────────────

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _sofaService.CancelBookingAsync(id, userId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ─── POST /api/sofa-bookings/{id}/status (Admin/Technician) ──────────

        [HttpPost("{id:int}/status")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] SofaUpdateStatusDto statusDto)
        {
            var result = await _sofaService.UpdateBookingStatusAsync(id, statusDto.Status);
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

    public class SofaUpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
