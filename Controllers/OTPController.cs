using CarWash.Api.Models.DTOs;
using CarWash.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CarWash.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OTPController : ControllerBase
    {
        private readonly IOTPService _otpService;
        private readonly ILogger<OTPController> _logger;

        public OTPController(IOTPService otpService, ILogger<OTPController> logger)
        {
            _otpService = otpService;
            _logger = logger;
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyOTPRequestDto request)
        {
            try
            {
                var isValid = await _otpService.VerifyOTPAsync(request);

                if (!isValid)
                    return BadRequest(new { Success = false, Message = "Invalid or expired OTP" });

                return Ok(new { Success = true, Message = "OTP verified successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP");
                return StatusCode(500, new { Success = false, Message = "An error occurred while verifying OTP" });
            }
        }

        [HttpPost("resend")]
        public async Task<IActionResult> Resend([FromBody] OTPRequestDto request)
        {
            try
            {
                request.IpAddress = GetIpAddress();

                // Check if can resend
                var canResend = await _otpService.CanResendOTPAsync(request.Type, request.Value);
                if (!canResend)
                    return BadRequest(new { Success = false, Message = "Please wait before requesting another OTP" });

                var result = await _otpService.GenerateAndSendOTPAsync(request);

                if (!result.Success)
                    return BadRequest(new { result.Success, result.Message });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending OTP");
                return StatusCode(500, new { Success = false, Message = "An error occurred while resending OTP" });
            }
        }

        [HttpGet("attempts/{type}/{value}")]
        public async Task<IActionResult> GetAttempts(string type, string value)
        {
            try
            {
                var attempts = await _otpService.GetOTPAttemptsAsync(type, value);
                return Ok(new { Success = true, Attempts = attempts });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OTP attempts");
                return StatusCode(500, new { Success = false, Message = "An error occurred" });
            }
        }

        private string GetIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}