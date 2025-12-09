// Entities/Service.cs
using System;
using System.Collections.Generic;

namespace CarWash.Api.Entities
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public int DurationInMinutes { get; set; }
        public string? Includes { get; set; } // JSON string
        public string? ImageUrl { get; set; }
        public bool IsPopular { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
        public string? AvailableSlots { get; set; } // JSON string
        public string? UnavailableDates { get; set; } // JSON string
        public int MaxBookingsPerSlot { get; set; } = 1;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}