// Entities/Service.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CarWash.Api.Entities
{
    [Table("Services")]
    public class WashingService
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(100)]
        public string SubCategory { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? DiscountedPrice { get; set; }

        [Required]
        public int DurationInMinutes { get; set; }

        // JSON serialized list of included features
        public string Includes { get; set; } = "[]";

        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsPopular { get; set; }

        // JSON serialized available time slots
        public string AvailableSlots { get; set; } = "[]";

        // JSON serialized unavailable dates
        public string UnavailableDates { get; set; } = "[]";

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; }
        //public string? AvailableSlots { get; set; } // JSON string
        //public string? UnavailableDates { get; set; } // JSON string
        public int MaxBookingsPerSlot { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        // public virtual ICollection<ServiceReview> Reviews { get; set; } = new List<ServiceReview>();

        [JsonIgnore]
        public virtual ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();

        // Helper properties (not mapped to database)
        [NotMapped]
        public decimal FinalPrice => DiscountedPrice ?? Price;

        [NotMapped]
        public double AverageRating { get; set; }

        [NotMapped]
        public int TotalReviews { get; set; }
    }
}