// Entities/OTP.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarWash.Api.Entities
{
    [Table("OTPs")]
    public class OTP
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Type { get; set; } = string.Empty; // "mobile" or "email"

        [Required]
        [MaxLength(255)]
        public string Value { get; set; } = string.Empty; // Mobile number or email

        // Add these properties for email/mobile tracking
        [MaxLength(255)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? MobileNumber { get; set; }

        [Required]
        [MaxLength(20)]
        public string Flow { get; set; } = string.Empty; // "login", "signup", "reset"

        public Guid? UserId { get; set; } // Make nullable since OTP can be sent before user creation

        public int Attempts { get; set; }
        public int MaxAttempts { get; set; } = 3;

        public bool IsUsed { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? VerifiedAt { get; set; }
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(10);

        // Optional: Device tracking
        [MaxLength(100)]
        public string? DeviceId { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [MaxLength(50)]
        public string? Purpose { get; set; }

        // Navigation property to User (nullable)
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}