using CarWash.Api.DTOs;
using CarWash.Api.Models.DTOs;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using System;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace CarWash.Api.Models.DTOs
{
    public class OTPResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ReferenceId { get; set; }
        public string? TempToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int RetryAfterSeconds { get; set; }
        public int AttemptsRemaining { get; set; }
        public bool CanResendInSeconds { get;set; }
        public int ResendCooldown { get; set; }
        // For development/testing only
        public string? OTPCode { get; set; }
    }
    public class OtpVerifiedResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public bool IsExpired { get; set; }
        public bool ExceededAttempts { get; set; }
        public string? AuthToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? SessionId { get; set; }
        public DateTime? TokenExpiresAt { get; set; }
        public UserDto? User { get; set; }
    }
    public class OtpDetailsDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? MobileNumber { get; set; }
        public string Flow { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public int Attempts { get; set; }
        public int MaxAttempts { get; set; }
        public bool IsUsed { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? DeviceId { get; set; }
        public string? IpAddress { get; set; }
        public string? Purpose { get; set; }

        // Helper properties
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        public int AttemptsRemaining => MaxAttempts - Attempts;
        public bool CanAttempt => !IsExpired && AttemptsRemaining > 0 && IsActive;
    }
    public class GenerateOtpDto
    {
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Flow { get; set; } = string.Empty;
        public string? DeviceId { get; set; }
        public string? IpAddress { get; set; }
        public string? Purpose { get; set; }
    }
    public class CreateOtpDto
    {
        [Required]
        [MaxLength(10)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Value { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? MobileNumber { get; set; }

        [Required]
        [MaxLength(20)]
        public string Flow { get; set; } = string.Empty;

        public Guid? UserId { get; set; }

        [MaxLength(100)]
        public string? DeviceId { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [MaxLength(50)]
        public string? Purpose { get; set; }

        public int MaxAttempts { get; set; } = 3;

        // Additional properties that might be useful
        public int ExpiryMinutes { get; set; } = 10;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(10);
    }
}




