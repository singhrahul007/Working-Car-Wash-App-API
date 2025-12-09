using CarWash.Api.DTOs;
using CarWash.Api.Interfaces;
using CarWash.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CarWash.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        private Guid GetUserId()
        {
            // Try different claim names that might contain the user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("userId")?.Value
                           ?? User.FindFirst("sub")?.Value
                           ?? User.FindFirst("uid")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException($"Invalid user ID format in token: {userIdClaim}");
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetUserId();
                var result = await _userService.GetProfileAsync(userId);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateDto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _userService.UpdateProfileAsync(userId, updateDto);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _userService.ChangePasswordAsync(userId, changePasswordDto);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("addresses")]
        public async Task<IActionResult> GetAddresses()
        {
            try
            {
                var userId = GetUserId();
                var result = await _userService.GetAddressesAsync(userId);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost("addresses")]
        public async Task<IActionResult> AddAddress([FromBody] AddressCreateDto addressDto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _userService.AddAddressAsync(userId, addressDto);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPut("addresses/{addressId:int}")]
        public async Task<IActionResult> UpdateAddress(int addressId, [FromBody] AddressCreateDto addressDto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _userService.UpdateAddressAsync(userId, addressId, addressDto);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpDelete("addresses/{addressId:int}")]
        public async Task<IActionResult> DeleteAddress(int addressId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _userService.DeleteAddressAsync(userId, addressId);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost("addresses/{addressId:int}/set-default")]
        public async Task<IActionResult> SetDefaultAddress(int addressId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _userService.SetDefaultAddressAsync(userId, addressId);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("two-factor")]
        public async Task<IActionResult> GetTwoFactorInfo()
        {
            try
            {
                var userId = GetUserId();
                var result = await _userService.GetTwoFactorInfoAsync(userId);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost("two-factor/setup")]
        public async Task<IActionResult> SetupTwoFactor([FromBody] TwoFactorSetupDto setupDto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _userService.SetupTwoFactorAsync(userId, setupDto);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost("two-factor/verify")]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] TwoFactorVerifyDto verifyDto)
        {
            try
            {
                var userId = GetUserId();
                var result = await _userService.VerifyTwoFactorAsync(userId, verifyDto);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost("two-factor/disable")]
        public async Task<IActionResult> DisableTwoFactor([FromBody] string currentPassword)
        {
            try
            {
                var userId = GetUserId();
                var result = await _userService.DisableTwoFactorAsync(userId, currentPassword);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}