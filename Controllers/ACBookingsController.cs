// Controllers/ACBookingsController.cs
using CarWash.Api.Models.DTOs;
using CarWash.Api.Models.DTOs.AC;
using CarWash.Api.Services.Interfaces;
using CarWash.Api.Services.Interfaces.AC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarWash.Api.Controllers
{
    [ApiController]
    [Route("api/ac-bookings")]
    //[Authorize(AuthenticationSchemes = "JwtBearer")]
    public class ACBookingsController : ControllerBase
    {
        private readonly IACServiceService _acServiceService;

        public ACBookingsController(IACServiceService acServiceService)
        {
            _acServiceService = acServiceService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ACBookingCreateDTOs bookingDto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized();

            var result = await _acServiceService.CreateBookingAsync(bookingDto, userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized();

            var result = await _acServiceService.GetUserBookingsAsync(userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized();

            var result = await _acServiceService.GetBookingByIdAsync(id, userId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized();

            var result = await _acServiceService.CancelBookingAsync(id, userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id}/status")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto statusDto)
        {
            var result = await _acServiceService.UpdateBookingStatusAsync(id, statusDto.Status);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}