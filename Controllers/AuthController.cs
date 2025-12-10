
using CarWash.Api.DTOs;
using CarWash.Api.Models.DTOs;
using CarWash.Api.Models.Responses;
using CarWash.Api.Services;
using CarWash.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarWash.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IVerificationService _verificationService;

        public AuthController(IAuthService authService, ILogger<AuthController> logger, IVerificationService verificationService)
        {
            _authService = authService;
            _logger = logger;
            _verificationService = verificationService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var result = await _authService.LoginAsync(request);

                return Ok(new AuthResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    AccessTokenExpiry = result.AccessTokenExpiry,
                    RefreshTokenExpiry = result.RefreshTokenExpiry,
                    User = result.User
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed");

                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        // Controllers/AuthController.cs
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new AuthResponseDto
                    {
                        Success = false,
                        Message = "Validation failed",
                        User = null
                    });
                }

                if (!request.AcceptTerms)
                {
                    return BadRequest(new AuthResponseDto
                    {
                        Success = false,
                        Message = "You must accept terms and conditions"
                    });
                }

                var result = await _authService.RegisterAsync(request);

                // Map DTO to Response
                return Ok(new AuthResponseDto
                {
                    Success = result.Success,
                    Message = result.Message,
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    AccessTokenExpiry = result.AccessTokenExpiry,
                    RefreshTokenExpiry = result.RefreshTokenExpiry,
                    SessionId = result.SessionId,
                    User = result.User != null ? new  UserDto
                    {
                        Id = result.User.Id,
                        Email = result.User.Email,
                        MobileNumber = result.User.MobileNumber,
                        FullName = result.User.FullName,
                        ProfilePicture = result.User.ProfilePicture,
                        ProfileImageUrl = result.User.ProfileImageUrl,
                        IsEmailVerified = result.User.IsEmailVerified,
                        IsMobileVerified = result.User.IsMobileVerified,
                        TwoFactorEnabled = result.User.TwoFactorEnabled,
                        TwoFactorMethod = result.User.TwoFactorMethod,
                        CreatedAt = result.User.CreatedAt,
                        LastLogin = result.User.LastLogin,
                        Roles = result.User.Roles
                    } : null,
                    RequiresOTP = result.RequiresOTP,
                    Requires2FA = result.Requires2FA,
                    TempToken = result.TempToken
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed");

                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto request)
        {
            try
            {
                var isValid = await _verificationService.VerifyEmailToken(request.Email, request.Token);

                if (isValid)
                {
                    // Now issue JWT tokens
                    var authResponse = await _authService.GenerateTokensAfterVerification(request.Email);

                    // Map DTO to Response
                    return Ok(new AuthResponseDto
                    {
                        Success = authResponse.Success,
                        Message = authResponse.Message,
                        AccessToken = authResponse.AccessToken,
                        RefreshToken = authResponse.RefreshToken,
                        AccessTokenExpiry = authResponse.AccessTokenExpiry,
                        RefreshTokenExpiry = authResponse.RefreshTokenExpiry,
                        SessionId = authResponse.SessionId,
                        User = authResponse.User != null ? new UserDto
                        {
                            Id = authResponse.User.Id,
                            Email = authResponse.User.Email,
                            MobileNumber = authResponse.User.MobileNumber,
                            FullName = authResponse.User.FullName,
                            ProfilePicture = authResponse.User.ProfilePicture,
                            ProfileImageUrl = authResponse.User.ProfileImageUrl,
                            IsEmailVerified = authResponse.User.IsEmailVerified,
                            IsMobileVerified = authResponse.User.IsMobileVerified,
                            TwoFactorEnabled = authResponse.User.TwoFactorEnabled,
                            TwoFactorMethod = authResponse.User.TwoFactorMethod,
                            CreatedAt = authResponse.User.CreatedAt,
                            LastLogin = authResponse.User.LastLogin,
                            Roles = authResponse.User.Roles
                        } : null
                    });
                }

                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid verification token"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email verification failed");
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = ex.Message
                });
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
        // Separate endpoint for verification
        //[HttpPost("verify-email")]
        //[AllowAnonymous]
        //public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        //{
        //    var isValid = await _verificationService.VerifyEmailToken(request.Email, request.Token);

        //    if (isValid)
        //    {
        //        // Update user as verified
        //        // Now issue JWT tokens
        //        var authResponse = await _authService.GenerateTokensAfterVerification(request.Email);

        //        return Ok(new AuthApiResponse
        //        {
        //            Success = true,
        //            Message = "Email verified successfully",
        //            AccessToken = authResponse.AccessToken,
        //            RefreshToken = authResponse.RefreshToken,
        //            User = authResponse.User
        //        });
        //    }

        //    return BadRequest(new { message = "Invalid verification token" });
        //}
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