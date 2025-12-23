// Services/Interfaces/IAuthService.cs
using CarWash.Api.Models.DTOs;
using CarWash.Api.Models.Entities;
using CarWash.Api.Utilities;
using System;
using System.Threading.Tasks;
using CarWash.Api.DTOs;

namespace CarWash.Api.Services.Interfaces
{
    public interface IAuthService
    {
        // Authentication methods
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto> VerifyOTPAndLoginAsync(VerifyOTPRequestDto request);
        Task<AuthResponseDto> SocialLoginAsync(SocialLoginRequestDto request);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string sessionId);
        Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request);
        Task<AuthResponseDto> GenerateTokensAfterVerification(string email);

        // OTP methods
        Task<OTPResponseDto> SendOTPAsync(OTPRequestDto request);
        Task<OTPResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request);
        Task<ServiceResult<bool>> VerifyOtpAsync(VerifyOtpDto verifyOtpDto);

        // User management
        Task<ServiceResult<UserDto>> GetCurrentUserAsync();
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);

        // Session management
        Task<bool> LogoutAsync(Guid userId, string? sessionId = null);
        Task<bool> LogoutAllAsync(Guid userId);
    }
}