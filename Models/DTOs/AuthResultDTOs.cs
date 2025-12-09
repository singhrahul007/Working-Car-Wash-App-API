// Models/DTOs/AuthResult.cs
using CarWash.Api.DTOs;
using System;

namespace CarWash.Api.Models.DTOs
{
    public class AuthResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public UserDto? User { get; set; }

        // Optional: Additional properties
        public bool RequiresOTP { get; set; }
        public string? OTPToken { get; set; }
        public string? SessionId { get; set; }

        // Constructor for success
        public static AuthResultDto SuccessResult(string token, string refreshToken, DateTime tokenExpiry, UserDto user)
        {
            return new AuthResultDto
            {
                Success = true,
                Message = "Authentication successful",
                Token = token,
                RefreshToken = refreshToken,
                TokenExpiry = tokenExpiry,
                User = user
            };
        }

        // Constructor for failure
        public static AuthResultDto FailureResult(string message)
        {
            return new AuthResultDto
            {
                Success = false,
                Message = message
            };
        }

        // Constructor for OTP required
        public static AuthResultDto OTPRequiredResult(string message, string otpToken)
        {
            return new AuthResultDto
            {
                Success = false,
                Message = message,
                RequiresOTP = true,
                OTPToken = otpToken
            };
        }
    }
}