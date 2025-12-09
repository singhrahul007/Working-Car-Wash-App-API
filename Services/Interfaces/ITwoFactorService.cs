// Services/Interfaces/ITwoFactorService.cs
using CarWash.Api.Models.DTOs;

namespace CarWash.Api.Services.Interfaces
{
    public interface ITwoFactorService
    {
        Task<OTPResult> GenerateAndSendOTPAsync(OTPRequestDto request);
        Task<bool> VerifyOTPAsync(VerifyOTPRequestDto request);
        Task<bool> VerifyTwoFactorCodeAsync(string userId, string code);
        Task<string> GenerateTwoFactorCodeAsync(string userId);

        string GenerateSecret();
        string GenerateQrCodeUrl(string email, string secret);
        bool VerifyCode(string secret, string code);
        List<string> GenerateBackupCodes();

    }
}