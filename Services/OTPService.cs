
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using CarWash.Api.Data;
using CarWash.Api.Services.Interfaces;
using CarWash.Api.Models.DTOs;
using CarWash.Api.Models.Entities;


namespace CarWash.Api.Services
{
    public class OTPService : IOTPService
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OTPService> _logger;
        private readonly Random _random = new Random();

        public OTPService(
            AppDbContext context,
            IDistributedCache cache,
            IEmailService emailService,
            ISmsService smsService,
            IConfiguration configuration,
            ILogger<OTPService> logger)
        {
            _context = context;
            _cache = cache;
            _emailService = emailService;
            _smsService = smsService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<OTPResponseDto> GenerateAndSendOTPAsync(OTPRequestDto request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.Type) || string.IsNullOrWhiteSpace(request.Value) || string.IsNullOrWhiteSpace(request.Flow))
                {
                    return new OTPResponseDto
                    {
                        Success = false,
                        Message = "Type, value, and flow are required"
                    };
                }

                // Validate type
                if (request.Type.ToLower() != "mobile" && request.Type.ToLower() != "email")
                {
                    return new OTPResponseDto
                    {
                        Success = false,
                        Message = "Invalid OTP type. Must be 'mobile' or 'email'"
                    };
                }

                // Validate value based on type
                if (request.Type.ToLower() == "mobile" && (request.Value.Length != 10 || !request.Value.All(char.IsDigit)))
                {
                    return new OTPResponseDto
                    {
                        Success = false,
                        Message = "Invalid mobile number. Must be 10 digits"
                    };
                }

                if (request.Type.ToLower() == "email" && !IsValidEmail(request.Value))
                {
                    return new OTPResponseDto
                    {
                        Success = false,
                        Message = "Invalid email address"
                    };
                }

                // Check if user can resend OTP
                if (!await CanResendOTPAsync(request.Type, request.Value))
                {
                    // Get remaining cooldown
                    var cacheKey = $"otp_cooldown_{request.Type}_{request.Value}";
                    var cooldownEnd = await _cache.GetStringAsync(cacheKey);

                    if (DateTime.TryParse(cooldownEnd, out var cooldownTime))
                    {
                        var secondsRemaining = (int)(cooldownTime - DateTime.UtcNow).TotalSeconds;
                        return new OTPResponseDto
                        {
                            Success = false,
                            Message = $"Please wait {secondsRemaining} seconds before requesting a new OTP",
                            CanResendInSeconds = true,
                            ResendCooldown = secondsRemaining
                        };
                    }
                }

                // Generate OTP code
                string otpCode;
                if (_configuration["OTP:UseTestOTP"] == "true" && !string.IsNullOrEmpty(_configuration["OTP:TestOTPCode"]))
                {
                    // Use test OTP for development
                    otpCode = _configuration["OTP:TestOTPCode"]!;
                    _logger.LogInformation("Using test OTP: {OTPCode}", otpCode);
                }
                else
                {
                    // Generate 6-digit OTP
                    otpCode = _random.Next(100000, 999999).ToString();
                }

                // Check for existing active OTP
                var existingOtp = await _context.OTPs
                    .FirstOrDefaultAsync(o =>
                        o.Type == request.Type &&
                        o.Value == request.Value &&
                        o.Flow == request.Flow &&
                        !o.IsUsed &&
                        !o.IsVerified &&
                        o.ExpiresAt > DateTime.UtcNow &&
                        o.IsActive);

                if (existingOtp != null)
                {
                    // Update existing OTP
                    existingOtp.Code = otpCode;
                    existingOtp.ExpiresAt = DateTime.UtcNow.AddMinutes(10);
                    existingOtp.CreatedAt = DateTime.UtcNow;
                    existingOtp.Attempts = 0;
                    existingOtp.DeviceId = request.DeviceId;
                    existingOtp.IpAddress = request.IpAddress;
                    existingOtp.Purpose = request.Flow;
                }
                else
                {
                    // Create new OTP
                    var otp = new OTP
                    {
                        Code = otpCode,
                        Type = request.Type.ToLower(),
                        Value = request.Value,
                        Flow = request.Flow.ToLower(),
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                        DeviceId = request.DeviceId,
                        IpAddress = request.IpAddress,
                        Purpose = request.Flow,
                        IsActive = true
                    };

                    _context.OTPs.Add(otp);
                }

                await _context.SaveChangesAsync();

                // Send OTP via appropriate channel
                bool sendSuccess = false;
                string sendMessage = string.Empty;

                if (request.Type.ToLower() == "mobile")
                {
                    sendSuccess = await SendOTPViaSMS(request.Value, otpCode, request.Flow);
                    sendMessage = sendSuccess ? "OTP sent via SMS" : "Failed to send SMS";
                }
                else if (request.Type.ToLower() == "email")
                {
                    sendSuccess = await SendOTPViaEmail(request.Value, otpCode, request.Flow);
                    sendMessage = sendSuccess ? "OTP sent via email" : "Failed to send email";
                }

                if (!sendSuccess)
                {
                    return new OTPResponseDto
                    {
                        Success = false,
                        Message = sendMessage
                    };
                }

                // Set cooldown cache (prevent spam)
                var cooldownKey = $"otp_cooldown_{request.Type}_{request.Value}";
                var cooldownSeconds = int.Parse(_configuration["OTP:ResendCooldownSeconds"] ?? "60");
                await _cache.SetStringAsync(cooldownKey,
                    DateTime.UtcNow.AddSeconds(cooldownSeconds).ToString("o"),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cooldownSeconds)
                    });

                // Generate temp token for verification
                var tempToken = GenerateTempToken(request.Type, request.Value, request.Flow);

                return new OTPResponseDto
                {
                    Success = true,
                    Message = $"OTP sent to your {request.Type}",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    TempToken = tempToken,
                    AttemptsRemaining = 3
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating OTP for {Type}: {Value}", request.Type, request.Value);
                return new OTPResponseDto
                {
                    Success = false,
                    Message = $"Failed to generate OTP: {ex.Message}"
                };
            }
        }

        public async Task<bool> VerifyOTPAsync(VerifyOTPRequestDto request)
        {
            try
            {
                return await ValidateOTPAsync(request.Type, request.Value, request.OTP, request.Flow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for {Type}: {Value}", request.Type, request.Value);
                return false;
            }
        }

        public async Task<bool> ValidateOTPAsync(string type, string value, string code, string flow)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(value) ||
                    string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(flow))
                {
                    _logger.LogWarning("Invalid OTP validation parameters");
                    return false;
                }

                // Find active OTP
                var otp = await _context.OTPs
                    .FirstOrDefaultAsync(o =>
                        o.Type == type.ToLower() &&
                        o.Value == value &&
                        o.Flow == flow.ToLower() &&
                        !o.IsUsed &&
                        !o.IsVerified &&
                        o.IsActive &&
                        o.ExpiresAt > DateTime.UtcNow);

                if (otp == null)
                {
                    _logger.LogWarning("No active OTP found for {Type}: {Value}", type, value);
                    return false;
                }

                // Check attempts
                if (otp.Attempts >= otp.MaxAttempts)
                {
                    _logger.LogWarning("OTP max attempts reached for {Type}: {Value}", type, value);
                    otp.IsActive = false; // Deactivate OTP after max attempts
                    await _context.SaveChangesAsync();
                    return false;
                }

                // Increment attempt count
                otp.Attempts++;
                await _context.SaveChangesAsync();

                // Verify code
                if (otp.Code != code)
                {
                    _logger.LogWarning("Invalid OTP code for {Type}: {Value}. Attempt {Attempts}/{MaxAttempts}",
                        type, value, otp.Attempts, otp.MaxAttempts);
                    return false;
                }

                // Mark OTP as used and verified
                otp.IsUsed = true;
                otp.IsVerified = true;
                otp.VerifiedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("OTP verified successfully for {Type}: {Value}", type, value);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating OTP for {Type}: {Value}", type, value);
                return false;
            }
        }

        public async Task<int> GetOTPAttemptsAsync(string type, string value)
        {
            try
            {
                var latestOtp = await _context.OTPs
                    .Where(o => o.Type == type.ToLower() && o.Value == value && o.IsActive)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();

                return latestOtp?.Attempts ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OTP attempts for {Type}: {Value}", type, value);
                return 0;
            }
        }

        public async Task<bool> CanResendOTPAsync(string type, string value)
        {
            try
            {
                var cacheKey = $"otp_cooldown_{type}_{value}";
                var cooldownEnd = await _cache.GetStringAsync(cacheKey);

                if (string.IsNullOrEmpty(cooldownEnd))
                {
                    return true;
                }

                if (DateTime.TryParse(cooldownEnd, out var cooldownTime))
                {
                    return DateTime.UtcNow >= cooldownTime;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking OTP resend cooldown for {Type}: {Value}", type, value);
                return false;
            }
        }

        #region Private Helper Methods

        private async Task<bool> SendOTPViaSMS(string phoneNumber, string otpCode, string flow)
        {
            try
            {
                var message = flow.ToLower() switch
                {
                    "login" => $"Your login OTP is {otpCode}. Valid for 10 minutes.",
                    "signup" => $"Your verification OTP is {otpCode}. Valid for 10 minutes.",
                    "reset" => $"Your password reset OTP is {otpCode}. Valid for 10 minutes.",
                    _ => $"Your OTP is {otpCode}. Valid for 10 minutes."
                };

                if (_configuration["SMS:MockMode"] == "true")
                {
                    _logger.LogInformation("Mock SMS sent to {PhoneNumber}: {Message}", phoneNumber, message);
                    return true;
                }
                else
                {
                    // Use the correct method name: SendSMSAsync (with capital SMS)
                    return await _smsService.SendSMSAsync(phoneNumber, message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP via SMS to {PhoneNumber}", phoneNumber);
                return false;
            }
        }
        private async Task<bool> SendOTPViaEmail(string email, string otpCode, string flow)
        {
            try
            {
                var subject = flow.ToLower() switch
                {
                    "login" => "Your Login OTP Code",
                    "signup" => "Verify Your Email Address",
                    "reset" => "Password Reset OTP",
                    _ => "Your OTP Code"
                };

                var body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px;'>
                        <h2>{subject}</h2>
                        <p>Your OTP code is: <strong>{otpCode}</strong></p>
                        <p>This code is valid for 10 minutes.</p>
                        <p>If you didn't request this OTP, please ignore this email.</p>
                        <hr>
                        <p style='color: #666; font-size: 12px;'>
                            This is an automated message from CarWash Service.
                        </p>
                    </div>";

                if (_configuration["Email:MockMode"] == "true")
                {
                    // Mock email sending for development
                    _logger.LogInformation("Mock email sent to {Email}: OTP {OTPCode}", email, otpCode);
                    return true;
                }
                else
                {
                    // Call actual email service
                    return await _emailService.SendEmailAsync(email, subject, body);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP via email to {Email}", email);
                return false;
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateTempToken(string type, string value, string flow)
        {
            // Generate a temporary token for OTP verification
            var data = $"{type}:{value}:{flow}:{DateTime.UtcNow.Ticks}";
            var bytes = System.Text.Encoding.UTF8.GetBytes(data);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        #endregion
    }
}