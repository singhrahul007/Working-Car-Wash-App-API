// Services/TwoFactorService.cs
using CarWash.Api.Models.DTOs;
using CarWash.Api.Services.Interfaces;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Collections.Generic; 

namespace CarWash.Api.Services
{
    public class TwoFactorService : ITwoFactorService
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<TwoFactorService> _logger;

        public TwoFactorService(
            IConfiguration configuration,
            IEmailService emailService,
            ISmsService smsService,
            ICacheService cacheService,
            ILogger<TwoFactorService> logger)
        {
            _configuration = configuration;
            _emailService = emailService;
            _smsService = smsService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<OTPResult> GenerateAndSendOTPAsync(OTPRequestDto request)
        {
            try
            {
                var otp = GenerateOTP();
                var cacheKey = $"otp:{request.Type}:{request.Value}:{request.Flow}";

                // Store OTP in cache with expiration
                await _cacheService.SetAsync(cacheKey, otp, TimeSpan.FromMinutes(10));

                // Send OTP based on type
                if (request.Type.ToLower() == "email")
                {
                    await _emailService.SendOTPEmailAsync(request.Value, otp, request.Flow);
                }
                else if (request.Type.ToLower() == "mobile")
                {
                    await _smsService.SendOTPSmsAsync(request.Value, otp, request.Flow);
                }
                else
                {
                    throw new ArgumentException($"Invalid OTP type: {request.Type}");
                }

                return new OTPResult
                {
                    Success = true,
                    Message = $"OTP sent to your {request.Type}",
                    // Don't return OTP in production - this is for testing only
                    OTP = _configuration["Environment"] == "Development" ? otp : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating and sending OTP");
                return new OTPResult
                {
                    Success = false,
                    Message = "Failed to send OTP. Please try again."
                };
            }
        }

        public async Task<bool> VerifyOTPAsync(VerifyOTPRequestDto request)
        {
            try
            {
                var cacheKey = $"otp:{request.Type}:{request.Value}:{request.Flow}";
                var storedOtp = await _cacheService.GetAsync<string>(cacheKey);

                if (string.IsNullOrEmpty(storedOtp) || storedOtp != request.OTP)
                    return false;

                // Remove OTP after successful verification
                await _cacheService.RemoveAsync(cacheKey);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP");
                return false;
            }
        }

       

        public async Task<bool> VerifyTwoFactorCodeAsync(string userId, string code)
        {
            try
            {
                var cacheKey = $"2fa:{userId}";
                var storedCode = await _cacheService.GetAsync<string>(cacheKey);

                return !string.IsNullOrEmpty(storedCode) && storedCode == code;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying 2FA code");
                return false;
            }
        }

        public async Task<string> GenerateTwoFactorCodeAsync(string userId)
        {
            try
            {
                var code = GenerateOTP();
                var cacheKey = $"2fa:{userId}";

                await _cacheService.SetAsync(cacheKey, code, TimeSpan.FromMinutes(5));

                return code;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating 2FA code");
                throw;
            }
        }
        public string GenerateSecret()
        {
            // Generate a random 32-character base32 secret for TOTP
            const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var random = new Random();
            var secret = new char[32];

            for (int i = 0; i < secret.Length; i++)
            {
                secret[i] = base32Chars[random.Next(base32Chars.Length)];
            }

            return new string(secret);
        }

        public string GenerateQrCodeUrl(string email, string secret)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(secret))
                return null;

            // Format: otpauth://totp/{issuer}:{email}?secret={secret}&issuer={issuer}
            var issuer = "CarWash Services";
            var encodedIssuer = Uri.EscapeDataString(issuer);
            var encodedEmail = Uri.EscapeDataString(email);

            return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secret}&issuer={encodedIssuer}";
        }


        public bool VerifyCode(string secret, string code)
        {
            try
            {
                // Simple TOTP verification (for demo)
                // In production, use a proper TOTP library like Otp.NET

                if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(code) || code.Length != 6)
                    return false;

                // TODO: Implement proper TOTP validation
                // For now, return true for testing (remove in production)
                _logger.LogWarning("Using placeholder 2FA verification - implement proper TOTP validation");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<string> GenerateBackupCodes()
        {
            var backupCodes = new List<string>();
            var random = new Random();

            // Generate 10 backup codes, each 8 digits
            for (int i = 0; i < 10; i++)
            {
                var code = random.Next(10000000, 99999999).ToString();
                backupCodes.Add(code);
            }

            return backupCodes;
        }

        // Helper method (keep this)
        private string GenerateOTP()
        {
            // More secure OTP generation
            var randomBytes = new byte[4];
            RandomNumberGenerator.Fill(randomBytes);

            // Generate a 6-digit OTP
            var otpNumber = Math.Abs(BitConverter.ToInt32(randomBytes, 0)) % 1000000;
            return otpNumber.ToString("D6"); // Ensures 6 digits with leading zeros
        }
    }


}

public class OTPResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? OTP { get; set; }
    }
