using System;
using System.Collections.Generic;

namespace CarWash.Api.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? MobileNumber { get; set; }
        public string? FullName { get; set; }
        public string? ProfilePicture { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsMobileVerified { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string? TwoFactorMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string[] Roles { get; set; } = Array.Empty<string>();
        // public List<string> Roles { get; set; } = new List<string>();

    }

    public class AddressDto
    {
        public int Id { get; set; }
        public string FullAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }

    public class AddressCreateDto
    {
        public string FullAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = "India";
        public string PostalCode { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class UpdateProfileDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? ProfilePicture { get; set; }
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class TwoFactorInfoDto
    {
        public bool IsEnabled { get; set; }
        public string? Method { get; set; }
        public string? SetupDate { get; set; }
        public string? QrCodeUrl { get; set; }
        public string? ManualEntryKey { get; set; }
        public List<string> BackupCodes { get; set; } = new();
    }
}