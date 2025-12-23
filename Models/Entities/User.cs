// Entities/User.cs
using CarWash.Api.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarWash.Api.Models.Entities
{
    [Table("Users")]
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [EmailAddress]
        [MaxLength(255)]
        public string? Email { get; set; }

        [Phone]
        [MaxLength(10)]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Mobile number must be 10 digits starting with 6, 7, 8, or 9")]
        public string MobileNumber { get; set; }
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

        [MaxLength(50)]
        public string? FullName { get; set; }

        [MaxLength(500)]
        public string? ProfilePicture { get; set; }

        [MaxLength(500)]
        public string? ProfileImageUrl { get; set; }

        // Add these missing properties
        public bool IsEmailVerified { get; set; }
        public bool IsMobileVerified { get; set; }

        public bool TwoFactorEnabled { get; set; }
        public string? TwoFactorMethod { get; set; } // "authenticator", "sms", "email"
        public string? TwoFactorSecret { get; set; }
        public string? BackupCodes { get; set; } // JSON serialized backup codes

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // For password reset and locking
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        public int LoginAttempts { get; set; }
        public DateTime? LockoutEnd { get; set; }

        public bool IsActive { get; set; } = true;
        public bool AcceptTerms { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // IP and device tracking
        public string? LastIpAddress { get; set; }
        public string? LastUserAgent { get; set; }

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<LoginSession> LoginSessions { get; set; } = new List<LoginSession>();
        public virtual ICollection<SocialAuth> SocialAuths { get; set; } = new List<SocialAuth>();
        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<ServiceReview> Reviews { get; set; } = new List<ServiceReview>();
        public virtual ICollection<OTP> OTPs { get; set; } = new List<OTP>();
    }
}