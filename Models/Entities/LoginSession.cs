using CarWash.Api.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace CarWash.Api.Models.Entities
{
    public class LoginSession
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SessionId { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string RefreshToken { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? DeviceType { get; set; } // mobile, web, tablet

        [MaxLength(100)]
        public string? DeviceName { get; set; }

        [MaxLength(50)]
        public string? OS { get; set; }

        [MaxLength(50)]
        public string? Browser { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual User User { get; set; } = null!;

        // Helper method
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    }
}