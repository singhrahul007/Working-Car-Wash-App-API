using CarWash.Api.Entities;
using System.ComponentModel.DataAnnotations;

namespace CarWash.Api.Models.Entities
{
    public class SocialAuth
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Provider { get; set; } = string.Empty; // google, facebook, apple

        [Required]
        [MaxLength(100)]
        public string ProviderId { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(255)]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? AccessToken { get; set; }

        [MaxLength(500)]
        public string? RefreshToken { get; set; }

        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUsed { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}