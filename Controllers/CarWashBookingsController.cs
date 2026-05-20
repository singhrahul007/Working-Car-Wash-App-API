using CarWash.Api.Models.DTOs.CarWash;
using CarWash.Api.Services.Interfaces.CarWash;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarWash.Api.Controllers
{
    [ApiController]
    [Route("api/car-bookings")]
    [Authorize(AuthenticationSchemes = "JwtBearer")]
    public class CarWashBookingsController : ControllerBase
    {
        private readonly ICarWashServiceService _carWashService;

        public CarWashBookingsController(ICarWashServiceService carWashService)
        {
            _carWashService = carWashService;
        }

        // ─── POST /api/car-bookings ───────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CarWashBookingCreateDTOs bookingDto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _carWashService.CreateBookingAsync(bookingDto, userId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ─── GET /api/car-bookings/my-bookings ───────────────────────────────

        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _carWashService.GetUserBookingsAsync(userId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ─── GET /api/car-bookings/{id} ──────────────────────────────────────

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _carWashService.GetBookingByIdAsync(id, userId.Value);
            return result.Success ? Ok(result) : NotFound(result);
        }

        // ─── POST /api/car-bookings/{id}/cancel ──────────────────────────────

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _carWashService.CancelBookingAsync(id, userId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ─── POST /api/car-bookings/{id}/status (Admin/Technician) ──────────

        [HttpPost("{id:int}/status")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] CarWashUpdateStatusDto statusDto)
        {
            var result = await _carWashService.UpdateBookingStatusAsync(id, statusDto.Status);
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

    public class CarWashUpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
