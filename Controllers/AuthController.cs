
using CarWash.Api.DTOs;
using CarWash.Api.Models.DTOs;
using CarWash.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


// Add these aliases for all conflicting DTOs
//using LoginRequest = CarWash.Api.Models.DTOs.LoginRequest;
//using RegisterRequest = CarWash.Api.Models.DTOs.RegisterRequest;
//using VerifyOTPRequest = CarWash.Api.Models.DTOs.VerifyOTPRequest;
//using OTPRequest = CarWash.Api.Models.DTOs.OTPRequest;
//using SocialLoginRequest = CarWash.Api.Models.DTOs.SocialLoginRequest;
//using ForgotPasswordRequest = CarWash.Api.Models.DTOs.ForgotPasswordRequest;
//using ResetPasswordRequest = CarWash.Api.Models.DTOs.ResetPasswordRequest;
namespace CarWash.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                // Get IP address and user agent from request
                request.IpAddress = GetIpAddress();
                request.UserAgent = GetUserAgent();

                var result = await _authService.LoginAsync(request);

                if (!result.Success)
                    return BadRequest(new { result.Success, result.Message });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { Success = false, Message = "An error occurred during login" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);

                if (!result.Success)
                    return BadRequest(new { result.Success, result.Message });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { Success = false, Message = "An error occurred during registration" });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOTP([FromBody] VerifyOTPRequestDto request)
        {
            try
            {
                var result = await _authService.VerifyOTPAndLoginAsync(request);

                if (!result.Success)
                    return BadRequest(new { result.Success, result.Message });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OTP verification");
                return StatusCode(500, new { Success = false, Message = "An error occurred during OTP verification" });
            }
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOTP([FromBody] OTPRequestDto request)
        {
            try
            {
                request.IpAddress = GetIpAddress();

                var result = await _authService.SendOTPAsync(request);

                if (!result.Success)
                    return BadRequest(new { result.Success, result.Message });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP");
                return StatusCode(500, new { Success = false, Message = "An error occurred while sending OTP" });
            }
        }

        [HttpPost("social-login")]
        public async Task<IActionResult> SocialLogin([FromBody] SocialLoginRequestDto request)
        {
            try
            {
                request.IpAddress = GetIpAddress();
                request.UserAgent = GetUserAgent();

                var result = await _authService.SocialLoginAsync(request);

                if (!result.Success)
                    return BadRequest(new { result.Success, result.Message });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during social login");
                return StatusCode(500, new { Success = false, Message = "An error occurred during social login" });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(request.RefreshToken, request.SessionId);

                if (!result.Success)
                    return Unauthorized(new { result.Success, result.Message });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, new { Success = false, Message = "An error occurred while refreshing token" });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = GetUserId();
                if (userId == Guid.Empty)
                    return Unauthorized();

                var sessionId = Request.Headers["X-Session-Id"].FirstOrDefault();
                await _authService.LogoutAsync(userId, sessionId);

                return Ok(new { Success = true, Message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { Success = false, Message = "An error occurred during logout" });
            }
        }

        [Authorize]
        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll()
        {
            try
            {
                var userId = GetUserId();
                if (userId == Guid.Empty)
                    return Unauthorized();

                await _authService.LogoutAllAsync(userId);

                return Ok(new { Success = true, Message = "Logged out from all devices" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout all");
                return StatusCode(500, new { Success = false, Message = "An error occurred during logout all" });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            try
            {
                var result = await _authService.ForgotPasswordAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in forgot password");
                return StatusCode(500, new { Success = false, Message = "An error occurred" });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(request);

                if (!result.Success)
                    return BadRequest(new { result.Success, result.Message });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return StatusCode(500, new { Success = false, Message = "An error occurred while resetting password" });
            }
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetUserId();
                if (userId == Guid.Empty)
                    return Unauthorized();

                var user = await _authService.GetUserByIdAsync(userId);
                if (user == null)
                    return NotFound(new { Success = false, Message = "User not found" });

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    MobileNumber = user.MobileNumber,
                    FullName = user.FullName,
                    ProfilePicture = user.ProfilePicture,
                    IsEmailVerified = user.IsEmailVerified,
                    IsMobileVerified = user.IsMobileVerified,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    CreatedAt = user.CreatedAt,
                    LastLogin = user.LastLogin
                };

                return Ok(new { Success = true, User = userDto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile");
                return StatusCode(500, new { Success = false, Message = "An error occurred while getting profile" });
            }
        }

        #region Helper Methods

        private string GetIpAddress()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Check for forwarded header (if behind proxy)
            if (HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].ToString();
            }

            return ipAddress ?? "Unknown";
        }

        private string GetUserAgent()
        {
            return HttpContext.Request.Headers.UserAgent.ToString();
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Guid.Empty;

            return userId;
        }

        #endregion
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
    }
}