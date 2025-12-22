
using CarWash.Api.Data;
using CarWash.Api.DTOs;
using CarWash.Api.Models.DTOs;
using CarWash.Api.Models.Responses;
using CarWash.Api.Services;
using CarWash.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CarWash.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IVerificationService _verificationService;
        private readonly AppDbContext _context;


        public AuthController(IAuthService authService, ILogger<AuthController> logger, IVerificationService verificationService,
            AppDbContext _context)
        {
            _authService = authService;
            _logger = logger;
            _verificationService = verificationService;
            _context = _context;
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
                if (!result.Success)

                    return BadRequest(result);
                return Ok(result);
                //return Ok(new AuthResponseDto
                //{
                //    Success = true,
                //    Message = "Login successful",
                //    AccessToken = result.AccessToken,
                //    RefreshToken = result.RefreshToken,
                //    AccessTokenExpiry = result.AccessTokenExpiry,
                //    RefreshTokenExpiry = result.RefreshTokenExpiry,
                //    User = result.User
                //});
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
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    //user.LastLogout = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
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
                    return Unauthorized(new { Success = false, Message = "Unauthorized" });

                var user = await _authService.GetUserByIdAsync(userId);
                if (user == null)
                    return NotFound(new { Success = false, Message = "User not found" });
                // Get user statistics (you'll need to implement these methods)
                var totalBookings = await _context.Bookings
                    .CountAsync(b => b.UserId == userId && b.Status != "Cancelled");
                //var loyaltyPoints = await _context.LoyaltyPoints
                //   .Where(lp => lp.UserId == userId && lp.IsActive)
                //   .SumAsync(lp => lp.Points);

                // Determine member tier based on points or bookings
                //var memberTier = loyaltyPoints >= 1000 ? "Gold" :
                //                loyaltyPoints >= 500 ? "Silver" : "Bronze";

                var userProfile = new UserProfileDto
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
                    LastLogin = user.LastLogin,
                    TotalBookings = totalBookings,
                   // LoyaltyPoints = loyaltyPoints,
                    //MemberTier = memberTier,
                    JoinedDateFormatted = user.CreatedAt.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)
                };

                return Ok(new { Success = true, User = userProfile });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile");
                return StatusCode(500, new { Success = false, Message = "An error occurred while getting profile" });
            }
        }
        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var userId = GetUserId();
                if (userId == Guid.Empty)
                    return Unauthorized();

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return NotFound(new { Success = false, Message = "User not found" });

                // Update fields if provided
                if (!string.IsNullOrWhiteSpace(request.FullName))
                    user.FullName = request.FullName;

                if (!string.IsNullOrWhiteSpace(request.MobileNumber))
                    user.MobileNumber = request.MobileNumber;

                if (!string.IsNullOrWhiteSpace(request.ProfilePicture))
                    user.ProfilePicture = request.ProfilePicture;

                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "Profile updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return StatusCode(500, new { Success = false, Message = "An error occurred while updating profile" });
            }
        }

        [Authorize]
        [HttpPost("upload-profile-picture")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            try
            {
                var userId = GetUserId();
                if (userId == Guid.Empty)
                    return Unauthorized();

                if (file == null || file.Length == 0)
                    return BadRequest(new { Success = false, Message = "No file uploaded" });

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                    return BadRequest(new { Success = false, Message = "File size too large (max 5MB)" });

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { Success = false, Message = "Invalid file type" });

                // Generate unique filename
                var fileName = $"{userId}_{Guid.NewGuid()}{extension}";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profile-pictures");

                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Update user profile picture in database
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    // Delete old profile picture if exists
                    if (!string.IsNullOrEmpty(user.ProfilePicture))
                    {
                        //var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicture);
                        //if (File.Exists(oldFilePath))
                        //    File.Delete(oldFilePath);
                    }

                    user.ProfilePicture = $"/uploads/profile-pictures/{fileName}";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                // Return the URL (adjust based on your server configuration)
                var fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/profile-pictures/{fileName}";

                return Ok(new
                {
                    Success = true,
                    Message = "Profile picture uploaded successfully",
                    ProfilePicture = fileUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile picture");
                return StatusCode(500, new { Success = false, Message = "An error occurred while uploading profile picture" });
            }
        }

        // Extend the existing Logout endpoint to also clear local storage data


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