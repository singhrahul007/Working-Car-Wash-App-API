using CarWash.Api.Data;
using CarWash.Api.DTOs;  // Make sure this is the only DTO namespace
using CarWash.Api.Entities;
using CarWash.Api.Interfaces;
using CarWash.Api.Models.DTOs;
using CarWash.Api.Models.Entities;
using CarWash.Api.Services.Interfaces;
using CarWash.Api.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace CarWash.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IOTPService _otpService;
        private readonly ISocialAuthService _socialAuthService;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<AuthService> _logger;
        private readonly bool _redisEnabled;
        public AuthService(
            AppDbContext context,
            IJwtService jwtService,
            IOTPService otpService,
            ISocialAuthService socialAuthService,
            IDistributedCache cache,
            IConfiguration configuration,
            IEmailService emailService,
            ISmsService smsService,
            IPasswordService passwordService,
            ILogger<AuthService> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _otpService = otpService;
            _socialAuthService = socialAuthService;
            _cache = cache;
            _emailService = emailService;
            _smsService = smsService;
            _configuration = configuration;
            _passwordService = passwordService;
            _logger = logger;
        }

        // Updated method signatures to use Dto suffix
        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            _logger.LogInformation($"Login attempt - Type: {request.LoginType}, IP: {request.IpAddress}");
            if (string.IsNullOrWhiteSpace(request.LoginType))
            {
                _logger.LogWarning("Login failed: Login type is required");
                return new AuthResponseDto { Success = false, Message = "Login type is required" };
            }
            if (request.LoginType.ToLower() == "mobile")
            {
                _logger.LogInformation($"Mobile login attempt for: {request.MobileNumber}");

                if (string.IsNullOrWhiteSpace(request.MobileNumber))
                {
                    _logger.LogWarning("Mobile login failed: Mobile number is required");
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Mobile number is required"
                    };
                }
                // Input validation
                if (string.IsNullOrWhiteSpace(request.LoginType))
                    return new AuthResponseDto { Success = false, Message = "Login type is required" };
            }
            if (request.LoginType.ToLower() == "mobile")
            {
                if (string.IsNullOrWhiteSpace(request.MobileNumber))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Mobile number is required"
                    };
                   
                }
                var cleanMobile = new string(request.MobileNumber.Where(char.IsDigit).ToArray());
                if (request.MobileNumber.Any(c => !char.IsDigit(c)))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Mobile number can only contain numbers"
                    };
                }

                // Check length
                if (request.MobileNumber.Length != 10)
                {
                        _logger.LogWarning($"Mobile login failed: Invalid length {cleanMobile.Length} for {cleanMobile}");
                        return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Mobile number must be 10 digits"
                    };
                }
                // Check if starts with valid digits (6, 7, 8, 9 for Indian numbers)
                var firstDigit = cleanMobile[0];
                if (firstDigit != '6' && firstDigit != '7' && firstDigit != '8' && firstDigit != '9')
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Mobile number must start with 6, 7, 8, or 9"
                    };
                }

                // Send OTP for mobile login
                var otpRequest = new OTPRequestDto
                {
                    Type = "mobile",
                    Value = cleanMobile,
                    Flow = "login",
                    DeviceId = request.DeviceId,
                    IpAddress = request.IpAddress
                };

                var otpResponse = await _otpService.GenerateAndSendOTPAsync(otpRequest);

                return new AuthResponseDto
                {
                    Success = otpResponse.Success,
                    Message = otpResponse.Message,
                    RequiresOTP = true,
                    TempToken = otpResponse.TempToken
                };
            }
            else if (request.LoginType.ToLower() == "email")
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                    return new AuthResponseDto { Success = false, Message = "Email and password are required" };

                // Find user by email
                var user = await _context.Users
                     .Include(u => u.UserRoles)
                     .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

                if (user == null)
                    return new AuthResponseDto { Success = false, Message = "Invalid email or password" };

                // Check if account is locked
                if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
                {
                    var remainingTime = (user.LockoutEnd.Value - DateTime.UtcNow).Minutes;
                    return new AuthResponseDto { Success = false, Message = $"Account is locked. Try again in {remainingTime} minutes." };
                }

                // Verify password
                if (!VerifyPassword(request.Password, user.PasswordHash))
                {
                    // Increment login attempts
                    user.LoginAttempts++;

                    // Lock account after 5 failed attempts
                    if (user.LoginAttempts >= 5)
                    {
                        user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                    }

                    await _context.SaveChangesAsync();
                    return new AuthResponseDto { Success = false, Message = "Invalid email or password" };
                }

                // Reset login attempts on successful login
                user.LoginAttempts = 0;
                user.LockoutEnd = null;
                user.LastLogin = DateTime.UtcNow;
                user.LastIpAddress = request.IpAddress;
                user.LastUserAgent = request.UserAgent;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Check if 2FA is enabled
                if (user.TwoFactorEnabled)
                {
                    // Generate temp token for 2FA
                    var tempToken = GenerateTempToken(user.Id.ToString());

                    return new AuthResponseDto
                    {
                        Success = true,
                        Message = "Two-factor authentication required",
                        Requires2FA = true,
                        TempToken = tempToken,
                        User = MapToUserDto(user)
                    };
                }

                // Create login session and return tokens
                var roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray();
                var authResponse = _jwtService.GenerateAuthResponse(user.Id, user.Email, roles);
                var session = await CreateLoginSessionAsync(user, request);
                var token = _jwtService.GenerateToken(user.MobileNumber.ToString() ?? user.Email ?? "user", user.Id);
                
                //var session = new LoginSession
                //{
                //    Id = Guid.NewGuid(),
                //    UserId = user.Id,
                //    SessionId = Guid.NewGuid().ToString(),
                //    RefreshToken = authResponse.RefreshToken,
                //    DeviceType = GetDeviceType(request.UserAgent),
                //    DeviceName = request.DeviceId,
                //    OS = GetOSFromUserAgent(request.UserAgent),
                //    Browser = GetBrowserFromUserAgent(request.UserAgent),
                //    IpAddress = request.IpAddress,
                //    UserAgent = request.UserAgent,
                //    IsActive = true,
                //    LastActivity = DateTime.UtcNow,
                //    ExpiresAt = authResponse.RefreshTokenExpiry,
                //    CreatedAt = DateTime.UtcNow
                //};
                //_context.LoginSessions.Add(session);
                //await _context.SaveChangesAsync();
                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    Token = authResponse.AccessToken,
                    AccessToken = authResponse.AccessToken,
                    RefreshToken = authResponse.RefreshToken,
                    AccessTokenExpiry = authResponse.AccessTokenExpiry,
                    RefreshTokenExpiry = authResponse.RefreshTokenExpiry,
                    SessionId = session.SessionId,
                    ExpiresAt = authResponse.AccessTokenExpiry,
                    User = MapToUserDto(user),
                    RequiresOTP = false,
                    Requires2FA = false
                };
            }

            return new AuthResponseDto { Success = false, Message = "Invalid login type" };
        }

        public async Task<AuthResponseDto> VerifyOTPAndLoginAsync(VerifyOTPRequestDto request)
        {
            // Verify OTP
            var isValid = await _otpService.VerifyOTPAsync(request);

            if (!isValid)
                return new AuthResponseDto { Success = false, Message = "Invalid or expired OTP" };

            // Find user by mobile or email
            User? user = null;
            if (request.Type.ToLower() == "mobile")
            {
                user = await _context.Users
                    .FirstOrDefaultAsync(u => u.MobileNumber == request.Value && u.IsActive);

                // If user doesn't exist and flow is login, create new user
                if (user == null && request.Flow.ToLower() == "login")
                {
                    // In a real app, you might want to handle this differently
                    return new AuthResponseDto { Success = false, Message = "User not found. Please sign up first." };
                }
            }
            else if (request.Type.ToLower() == "email")
            {
                user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Value && u.IsActive);
            }

            if (user == null)
                return new AuthResponseDto { Success = false, Message = "User not found" };

            // Update verification status
            if (request.Type.ToLower() == "mobile")
            {
                user.IsMobileVerified = true;
            }
            else if (request.Type.ToLower() == "email")
            {
                user.IsEmailVerified = true;
            }

            user.LastLogin = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Create login session
            var session = await CreateLoginSessionAsync(user, new LoginRequestDto
            {
                IpAddress = "N/A", // You should pass actual IP from request context
                UserAgent = "N/A",
                DeviceId = "N/A"
            });

            var token = _jwtService.GenerateToken(user.MobileNumber.ToString() ?? user.Email ?? "user", user.Id);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                RefreshToken = session.RefreshToken,
                SessionId = session.SessionId,
                ExpiresAt = session.ExpiresAt,
                User = MapToUserDto(user)
            };
        }

        public async Task<AuthResponseDto> SocialLoginAsync(SocialLoginRequestDto request)
        {
            try
            {
                // Verify social token and get user info
                User? user = null;

                switch (request.Provider.ToLower())
                {
                    case "google":
                        var googleUser = await _socialAuthService.VerifyGoogleTokenAsync(request.Token);
                        if (googleUser == null)
                            return new AuthResponseDto { Success = false, Message = "Invalid Google token" };

                        user = await GetOrCreateSocialUser("google", googleUser.Id, googleUser.Email, googleUser.Name);
                        break;

                    case "facebook":
                        var facebookUser = await _socialAuthService.VerifyFacebookTokenAsync(request.Token);
                        if (facebookUser == null)
                            return new AuthResponseDto { Success = false, Message = "Invalid Facebook token" };

                        user = await GetOrCreateSocialUser("facebook", facebookUser.Id, facebookUser.Email, facebookUser.Name);
                        break;

                    case "apple":
                        var appleUser = await _socialAuthService.VerifyAppleTokenAsync(request.Token);
                        if (appleUser == null)
                            return new AuthResponseDto { Success = false, Message = "Invalid Apple token" };

                        user = await GetOrCreateSocialUser("apple", appleUser.Id, appleUser.Email, appleUser.Name);
                        break;

                    default:
                        return new AuthResponseDto { Success = false, Message = "Unsupported social provider" };
                }

                if (user == null)
                    return new AuthResponseDto { Success = false, Message = "Failed to authenticate with social provider" };

                // Create login session
                var session = await CreateLoginSessionAsync(user, new LoginRequestDto
                {
                    IpAddress = request.IpAddress,
                    UserAgent = request.UserAgent,
                    DeviceId = request.DeviceId
                });

                var token = _jwtService.GenerateToken(user.MobileNumber.ToString() ?? user.Email ?? "user", user.Id);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Social login successful",
                    Token = token,
                    RefreshToken = session.RefreshToken,
                    SessionId = session.SessionId,
                    ExpiresAt = session.ExpiresAt,
                    User = MapToUserDto(user)
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto { Success = false, Message = $"Social login failed: {ex.Message}" };
            }
        }
        // Services/AuthService.cs - Fix the RegisterAsync method
        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email ||
                                             (request.MobileNumber != null && u.MobileNumber == request.MobileNumber));

                if (existingUser != null)
                {
                    throw new Exception(existingUser.Email == request.Email
                        ? "Email already registered"
                        : "Mobile number already registered");
                }
                var cleanMobile = new string(request.MobileNumber.Where(char.IsDigit).ToArray());
                // Check if contains any non-digit characters
                if (request.MobileNumber.Any(c => !char.IsDigit(c)))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Mobile number can only contain numbers"
                    };
                }

                // Check length
                if (request.MobileNumber.Length != 10)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Mobile number must be 10 digits"
                    };
                }
                // Check if starts with valid digits (6, 7, 8, 9 for Indian numbers)
                var firstDigit = cleanMobile[0];
                if (firstDigit != '6' && firstDigit != '7' && firstDigit != '8' && firstDigit != '9')
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Mobile number must start with 6, 7, 8, or 9"
                    };
                }

                // Create password hash
                var (hash, salt) = _passwordService.CreatePasswordHash(request.Password);

                // Create new user
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    MobileNumber = request.MobileNumber,
                    FullName = request.FullName,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    IsActive = true,
                    AcceptTerms = request.AcceptTerms,
                    CreatedAt = DateTime.UtcNow,
                    IsEmailVerified = false,
                    IsMobileVerified = false
                };

                // Get or create customer role
                var customerRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == "customer");

                if (customerRole == null)
                {
                    customerRole = new Role
                    {
                        Name = "customer",
                        Description = "Regular customer"
                    };
                    _context.Roles.Add(customerRole);
                    await _context.SaveChangesAsync();
                }

                // Assign customer role to user
                user.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = customerRole.Id,
                    AssignedAt = DateTime.UtcNow
                });

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                // Fetch user with roles
                var createdUser = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == user.Id);
                // Generate JWT tokens
                var roles = createdUser?.UserRoles.Select(ur => ur.Role.Name).ToList();
                var authResponse = _jwtService.GenerateAuthResponse(createdUser.Id, createdUser?.Email, roles);


                // Create login session
                var loginSession = new LoginSession
                {
                    Id = Guid.NewGuid(),
                    UserId = createdUser.Id,
                    SessionId = Guid.NewGuid().ToString(),
                    RefreshToken = authResponse.RefreshToken,
                    DeviceType = "mobile",
                    DeviceName = "mobile",
                    IsActive = true,
                    LastActivity = DateTime.UtcNow,
                    ExpiresAt = authResponse.RefreshTokenExpiry,
                    CreatedAt = DateTime.UtcNow
                };

                _context.LoginSessions.Add(loginSession);

                // Update user's last login
                user.LastLogin = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Build user profile response
                authResponse.User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    MobileNumber = user.MobileNumber,
                    ProfileImageUrl = user.ProfileImageUrl,
                    IsEmailVerified = user.IsEmailVerified,
                    IsMobileVerified = user.IsMobileVerified,
                    Roles = roles.ToArray(),
                    CreatedAt = user.CreatedAt
                };
                authResponse.SessionId = loginSession.SessionId;
                authResponse.Token = authResponse.AccessToken;
                authResponse.ExpiresAt = authResponse.AccessTokenExpiry;
                authResponse.Success = true;
                authResponse.Message = "Registration successful";

                //  _logger.LogInformation("Registration completed successfully for user: {Email}", request.Email);

                return authResponse;
            }
            catch (Exception ex)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = $"Registration failed: {ex.Message}"
                };
            }
           
        }
    
        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string sessionId)
        {
            var session = await _context.LoginSessions
                .Include(ls => ls.User)
                .FirstOrDefaultAsync(ls =>
                    ls.SessionId == sessionId &&
                    ls.RefreshToken == refreshToken &&
                    ls.IsActive &&
                    !ls.IsExpired);

            if (session == null || session.User == null)
                return new AuthResponseDto { Success = false, Message = "Invalid or expired session" };

            // Update session activity
            session.LastActivity = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate new access token
            var newToken = _jwtService.GenerateToken(
                session.User.MobileNumber.ToString() ?? session.User.Email ?? "user",
                session.User.Id);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Token refreshed",
                Token = newToken,
                RefreshToken = session.RefreshToken,
                SessionId = session.SessionId,
                ExpiresAt = session.ExpiresAt
            };
        }

        public async Task<bool> LogoutAsync(Guid userId, string? sessionId = null)
        {
            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                // Logout specific session
                var session = await _context.LoginSessions
                    .FirstOrDefaultAsync(ls => ls.UserId == userId && ls.SessionId == sessionId);

                if (session != null)
                {
                    session.IsActive = false;
                    await _context.SaveChangesAsync();
                }
            }

            return true;
        }

        public async Task<bool> LogoutAllAsync(Guid userId)
        {
            var sessions = await _context.LoginSessions
                .Where(ls => ls.UserId == userId && ls.IsActive)
                .ToListAsync();

            foreach (var session in sessions)
            {
                session.IsActive = false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<OTPResponseDto> SendOTPAsync(OTPRequestDto request)
        {
            return await _otpService.GenerateAndSendOTPAsync(request);
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            if (!VerifyPassword(currentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<OTPResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            // Find user by email or mobile
            User? user = null;

            if (request.Type.ToLower() == "email")
            {
                user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Value && u.IsActive);
            }
            else if (request.Type.ToLower() == "mobile")
            {
                user = await _context.Users
                    .FirstOrDefaultAsync(u => u.MobileNumber.ToString() == request.Value && u.IsActive);
            }

            if (user == null)
            {
                // For security, don't reveal if user exists
                return new OTPResponseDto
                {
                    Success = true,
                    Message = "If an account exists, you will receive a reset code",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10)
                };
            }

            // Generate and send OTP
            var otpRequest = new OTPRequestDto
            {
                Type = request.Type,
                Value = request.Value,
                Flow = "reset"
            };

            return await _otpService.GenerateAndSendOTPAsync(otpRequest);
        }

        public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            // Verify reset token (could be OTP or JWT token)
            // Implementation depends on your reset token strategy

            // For demo, we'll assume request.Token is an OTP
            var cacheKey = $"reset_token_{request.Token}";
            var userIdStr = await _cache.GetStringAsync(cacheKey);

            if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return new AuthResponseDto { Success = false, Message = "Invalid or expired reset token" };

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return new AuthResponseDto { Success = false, Message = "User not found" };

            // Update password
            user.PasswordHash = HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Clear reset token from cache
            await _cache.RemoveAsync(cacheKey);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Password reset successful"
            };
        }

        // Missing interface methods
        public async Task<ServiceResult<bool>> VerifyOtpAsync(VerifyOtpDto verifyOtpDto)
        {
            try
            {
                // Implement OTP verification logic
                var isValid = await _otpService.VerifyOTPAsync(new VerifyOTPRequestDto
                {
                    Type = verifyOtpDto.Type,
                    Value = verifyOtpDto.Value,
                    OTP = verifyOtpDto.OTP,
                    Flow = verifyOtpDto.Flow ?? "verification"
                });

                return ServiceResult<bool>.SuccessResult(isValid);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult($"Failed to verify OTP: {ex.Message}");
            }
        }

        public async Task<ServiceResult<UserDto>> GetCurrentUserAsync()
        {
            try
            {
                // This method typically gets the current user from HttpContext
                // For now, return a placeholder or implement based on your authentication
                return ServiceResult<UserDto>.FailureResult("Not implemented");
            }
            catch (Exception ex)
            {
                return ServiceResult<UserDto>.FailureResult($"Failed to get current user: {ex.Message}");
            }
        }
        public async Task<AuthResponseDto> GenerateTokensAfterVerification(string email)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !user.IsActive)
                throw new Exception("User not found or inactive");

            // Mark email as verified (if not already)
            user.IsEmailVerified = true;
            await _context.SaveChangesAsync();
            //Get roles
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray();
            var authResponse =  _jwtService.GenerateAuthResponse(user.Id, user.Email, roles);

            // Create login session
            var loginSession = new LoginSession
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                SessionId = Guid.NewGuid().ToString(),
                RefreshToken = authResponse.RefreshToken,
                DeviceType = "web",
                DeviceName = "Web Browser",
                IsActive = true,
                LastActivity = DateTime.UtcNow,
                ExpiresAt = authResponse.RefreshTokenExpiry,
                CreatedAt = DateTime.UtcNow
            };
            _context.LoginSessions.Add(loginSession);
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            // Set user profile
            authResponse.User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                FullName = user.FullName,
                ProfileImageUrl = user.ProfileImageUrl,
                IsEmailVerified = user.IsEmailVerified,
                IsMobileVerified = user.IsMobileVerified,
                TwoFactorEnabled = user.TwoFactorEnabled,
                TwoFactorMethod = user.TwoFactorMethod,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin,
                Roles = roles.ToArray()
            };
            authResponse.SessionId = loginSession.SessionId;
            authResponse.Token = authResponse.AccessToken;
            authResponse.ExpiresAt = authResponse.AccessTokenExpiry;

            return authResponse;
        }
        #region Private Helper Methods

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var salt = _configuration["PasswordSalt"] ?? "default_salt";
            var bytes = Encoding.UTF8.GetBytes(password + salt);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            var hashToVerify = HashPassword(password);
            return passwordHash == hashToVerify;
        }

        private async Task<LoginSession> CreateLoginSessionAsync(User user, LoginRequestDto request)
        {
            var sessionId = GenerateSessionId();
            var refreshToken = GenerateRefreshToken();

            var session = new LoginSession
            {
                UserId = user.Id,
                SessionId = sessionId,
                RefreshToken = refreshToken,
                DeviceType = GetDeviceType(request.UserAgent),
                DeviceName = request.DeviceId,
                OS = GetOSFromUserAgent(request.UserAgent),
                Browser = GetBrowserFromUserAgent(request.UserAgent),
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                ExpiresAt = DateTime.UtcNow.AddDays(7), // Refresh token expires in 7 days
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow
            };

            _context.LoginSessions.Add(session);
            await _context.SaveChangesAsync();

            return session;
        }

        private async Task<User?> GetOrCreateSocialUser(string provider, string providerId, string email, string name)
        {
            // Check if social auth already exists
            var socialAuth = await _context.SocialAuths
                .Include(sa => sa.User)
                .FirstOrDefaultAsync(sa => sa.Provider == provider && sa.ProviderId == providerId);

            if (socialAuth != null)
            {
                // Update last used
                socialAuth.LastUsed = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return socialAuth.User;
            }

            // Check if user with email already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
            {
                // Link social auth to existing user
                var newSocialAuth = new SocialAuth
                {
                    UserId = existingUser.Id,
                    Provider = provider,
                    ProviderId = providerId,
                    Email = email,
                    CreatedAt = DateTime.UtcNow,
                    LastUsed = DateTime.UtcNow
                };

                _context.SocialAuths.Add(newSocialAuth);
                await _context.SaveChangesAsync();
                return existingUser;
            }

            // Create new user
            var user = new User
            {
                Email = email,
                FullName = name,
                IsEmailVerified = true, // Social logins typically verify email
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create social auth
            var socialAuthNew = new SocialAuth
            {
                UserId = user.Id,
                Provider = provider,
                ProviderId = providerId,
                Email = email,
                CreatedAt = DateTime.UtcNow,
                LastUsed = DateTime.UtcNow
            };

            _context.SocialAuths.Add(socialAuthNew);
            await _context.SaveChangesAsync();

            return user;
        }

        private string GenerateSessionId()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("/", "_")
                .Replace("+", "-")
                .Replace("=", "");
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string GenerateTempToken(string userId)
        {
            // Simple temp token for 2FA flow
            var data = $"{userId}_{DateTime.UtcNow.Ticks}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
        }

        private string GetDeviceType(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent)) return "unknown";

            if (userAgent.Contains("Mobile")) return "mobile";
            if (userAgent.Contains("Tablet")) return "tablet";
            return "web";
        }

        private string GetOSFromUserAgent(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent)) return "Unknown";

            if (userAgent.Contains("Android")) return "Android";
            if (userAgent.Contains("iPhone") || userAgent.Contains("iPad")) return "iOS";
            if (userAgent.Contains("Windows")) return "Windows";
            if (userAgent.Contains("Mac")) return "macOS";
            if (userAgent.Contains("Linux")) return "Linux";

            return "Unknown";
        }

        private string GetBrowserFromUserAgent(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent)) return "Unknown";

            if (userAgent.Contains("Chrome")) return "Chrome";
            if (userAgent.Contains("Firefox")) return "Firefox";
            if (userAgent.Contains("Safari")) return "Safari";
            if (userAgent.Contains("Edge")) return "Edge";

            return "Unknown";
        }

        private UserDto MapToUserDto(User user)
        {
            return new UserDto
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
        }

        #endregion
    }
}