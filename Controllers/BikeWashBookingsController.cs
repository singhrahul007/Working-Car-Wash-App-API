using CarWash.Api.Models.DTOs.BikeWash;
using CarWash.Api.Services.Interfaces.BikeWash;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarWash.Api.Controllers
{
    [ApiController]
    [Route("api/bike-bookings")]
    [Authorize(AuthenticationSchemes = "JwtBearer")]
    public class BikeWashBookingsController : ControllerBase
    {
        private readonly IBikeWashServiceService _bikeWashService;

        public BikeWashBookingsController(IBikeWashServiceService bikeWashService)
        {
            _bikeWashService = bikeWashService;
        }

        // ─── POST /api/bike-bookings ───────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BikeWashBookingCreateDTOs bookingDto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _bikeWashService.CreateBookingAsync(bookingDto, userId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ─── GET /api/bike-bookings/my-bookings ───────────────────────────────

        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _bikeWashService.GetUserBookingsAsync(userId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ─── GET /api/bike-bookings/{id} ──────────────────────────────────────

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _bikeWashService.GetBookingByIdAsync(id, userId.Value);
            return result.Success ? Ok(result) : NotFound(result);
        }

        // ─── POST /api/bike-bookings/{id}/cancel ──────────────────────────────

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _bikeWashService.CancelBookingAsync(id, userId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ─── POST /api/bike-bookings/{id}/status (Admin/Technician) ──────────

        [HttpPost("{id:int}/status")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] BikeWashUpdateStatusDto statusDto)
        {
            var result = await _bikeWashService.UpdateBookingStatusAsync(id, statusDto.Status);
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

    public class BikeWashUpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
