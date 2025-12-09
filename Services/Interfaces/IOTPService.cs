using CarWash.Api.Models.DTOs;
using CarWash.Api.Models.Entities;

namespace CarWash.Api.Services.Interfaces
{
    public interface IOTPService
    {
        Task<OTPResponseDto> GenerateAndSendOTPAsync(OTPRequestDto request);
        Task<bool> VerifyOTPAsync(VerifyOTPRequestDto request);
        Task<bool> ValidateOTPAsync(string type, string value, string code, string flow);
        Task<int> GetOTPAttemptsAsync(string type, string value);
        Task<bool> CanResendOTPAsync(string type, string value);
    }
}