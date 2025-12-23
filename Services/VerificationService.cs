using CarWash.Api.Data;
using CarWash.Api.Models.Entities;
using CarWash.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarWash.Api.Services
{
    public class VerificationService : IVerificationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VerificationService> _logger;

        public VerificationService(AppDbContext context, ILogger<VerificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> GenerateEmailVerificationToken(string email)
        {
            // Generate a secure token
            var token = Guid.NewGuid().ToString("N");

            // Save to database
            var otp = new OTP
            {
                Code = token,
                Type = "email",
                Email = email,
                Flow = "verification",
                ExpiresAt = DateTime.UtcNow.AddHours(24), // 24 hours expiry
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.OTPs.Add(otp);
            await _context.SaveChangesAsync();

            // TODO: Send verification email
            await SendVerificationEmail(email, token);

            return token;
        }

        public async Task<string> GenerateMobileVerificationCode(string mobileNumber)
        {
            // Generate 6-digit code
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();

            // Save to database
            var otp = new OTP
            {
                Code = code,
                Type = "mobile",
                MobileNumber = mobileNumber,
                Flow = "verification",
                ExpiresAt = DateTime.UtcNow.AddMinutes(10), // 10 minutes expiry
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.OTPs.Add(otp);
            await _context.SaveChangesAsync();

            // TODO: Send SMS
            await SendVerificationSms(mobileNumber, code);

            return code;
        }

        public async Task<bool> VerifyEmailToken(string email, string token)
        {
            var otp = await _context.OTPs
                .Where(o => o.Email == email &&
                           o.Code == token &&
                           o.Type == "email" &&
                           o.Flow == "verification" &&
                           o.IsActive &&
                           o.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (otp == null)
                return false;

            // Mark OTP as used
            otp.IsUsed = true;
            otp.IsVerified = true;
            otp.VerifiedAt = DateTime.UtcNow;
            otp.IsActive = false;

            await _context.SaveChangesAsync();

            // Update user verification status
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                user.IsEmailVerified = true;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> VerifyMobileCode(string mobileNumber, string code)
        {
            var otp = await _context.OTPs
                .Where(o => o.MobileNumber == mobileNumber &&
                           o.Code == code &&
                           o.Type == "mobile" &&
                           o.Flow == "verification" &&
                           o.IsActive &&
                           o.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (otp == null)
                return false;

            // Mark OTP as used
            otp.IsUsed = true;
            otp.IsVerified = true;
            otp.VerifiedAt = DateTime.UtcNow;
            otp.IsActive = false;

            await _context.SaveChangesAsync();

            // Update user verification status
            var user = await _context.Users.FirstOrDefaultAsync(u => u.MobileNumber == mobileNumber);
            if (user != null)
            {
                user.IsMobileVerified = true;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> SendVerificationEmail(string email, string token)
        {
            try
            {
                // TODO: Implement email sending
                // For now, log the token
                _logger.LogInformation("Verification email sent to {Email} with token: {Token}", email, token);

                // Create verification link
                var verificationLink = $"https://yourdomain.com/verify-email?email={email}&token={token}";
                _logger.LogInformation("Verification link: {Link}", verificationLink);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendVerificationSms(string mobileNumber, string code)
        {
            try
            {
                // TODO: Implement SMS sending
                _logger.LogInformation("Verification SMS sent to {MobileNumber} with code: {Code}", mobileNumber, code);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification SMS to {MobileNumber}", mobileNumber);
                return false;
            }
        }
    }
}

