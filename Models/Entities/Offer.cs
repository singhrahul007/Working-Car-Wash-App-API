using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarWash.Api.Models.Entities
{
    public class Offer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string DiscountType { get; set; } = string.Empty; // "percentage" or "fixed"

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountValue { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal MinAmount { get; set; }

        public DateTime ValidFrom { get; set; } = DateTime.UtcNow;
        public DateTime ValidUntil { get; set; }

        [MaxLength(50)]
        public string Category { get; set; } = "all"; // "all", "car-wash", "ac-service", etc.

        [MaxLength(500)]
        public string ApplicableServices { get; set; } = "[\"all\"]"; // JSON array of service categories

        [MaxLength(100)]
        public string Icon { get; set; } = "gift";

        [MaxLength(50)]
        public string ColorCode { get; set; } = "#2196F3";

        [MaxLength(1000)]
        public string Terms { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public int UsageLimit { get; set; } = 1000;
        public int UsedCount { get; set; } = 0;
        public bool IsOneTimePerUser { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Add this for tracking
        public bool IsDeleted { get; set; } = false;
    }
}