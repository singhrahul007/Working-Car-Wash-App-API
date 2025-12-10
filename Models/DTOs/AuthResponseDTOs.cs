using System;

namespace CarWash.Api.DTOs
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public string? SessionId { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public UserDto? User { get; set; }
        public bool RequiresOTP { get; set; }
        public bool Requires2FA { get; set; }
        public string? TempToken { get; set; }
        public string AccessToken { get; set; } = string.Empty;
       
        public DateTime? AccessTokenExpiry { get; set; } 
        public DateTime? RefreshTokenExpiry { get; set; } 
    }

}