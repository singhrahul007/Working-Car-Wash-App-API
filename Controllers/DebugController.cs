// Add this temporary controller to help debug
// Remove it in production!

using Microsoft.AspNetCore.Mvc;
using CarWash.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace CarWash.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DebugController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public DebugController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("latest-otp/{mobile}")]
        public async Task<IActionResult> GetLatestOtp(string mobile)
        {
            var otp = await _context.OTPs
                .Where(o => o.Value == mobile && o.Type == "mobile")
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.Code,
                    o.Type,
                    o.Value,
                    o.Flow,
                    o.IsUsed,
                    o.IsVerified,
                    o.IsActive,
                    o.Attempts,
                    o.MaxAttempts,
                    o.CreatedAt,
                    o.ExpiresAt,
                    IsExpired = o.ExpiresAt < DateTime.UtcNow,
                    CanAttempt = !o.IsUsed && o.Attempts < o.MaxAttempts && o.ExpiresAt > DateTime.UtcNow && o.IsActive
                })
                .FirstOrDefaultAsync();

            if (otp == null)
                return NotFound(new { message = "No OTP found for this mobile number" });

            return Ok(otp);
        }

        [HttpGet("check-user/{mobile}")]
        public async Task<IActionResult> CheckUser(string mobile)
        {
            var user = await _context.Users
                .Where(u => u.MobileNumber == mobile)
                .Select(u => new
                {
                    u.Id,
                    u.MobileNumber,
                    u.Email,
                    u.FullName,
                    u.IsMobileVerified,
                    u.IsEmailVerified,
                    u.IsActive,
                    u.CreatedAt,
                    u.LastLogin
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "No user found with this mobile number" });

            return Ok(user);
        }

        [HttpGet("otp-config")]
        public IActionResult GetOtpConfig()
        {
            return Ok(new
            {
                UseTestOTP = _configuration["OTP:UseTestOTP"],
                TestOTPCode = _configuration["OTP:TestOTPCode"],
                ResendCooldownSeconds = _configuration["OTP:ResendCooldownSeconds"],
                SMSMockMode = _configuration["SMS:MockMode"],
                EmailMockMode = _configuration["Email:MockMode"]
            });
        }

        [HttpGet("all-otps")]
        public async Task<IActionResult> GetAllOtps()
        {
            var otps = await _context.OTPs
                .OrderByDescending(o => o.CreatedAt)
                .Take(20)
                .Select(o => new
                {
                    o.Code,
                    o.Type,
                    o.Value,
                    o.Flow,
                    o.IsUsed,
                    o.IsActive,
                    o.CreatedAt,
                    o.ExpiresAt
                })
                .ToListAsync();

            return Ok(otps);
        }
    }
}