using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarWash.Api.Models.Entities
{
    public class Slot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(10)]
        public string StartTime { get; set; } // Format: "09:00", "14:30"

        [Required]
        [StringLength(10)]
        public string EndTime { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Required]
        public int MaxCapacity { get; set; } // Maximum bookings allowed

        [Required]
        public int CurrentBookings { get; set; } = 0; // Current booked count
        [Timestamp]
        public byte[] RowVersion { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; }

        // Computed property for availability
        [NotMapped]
        public bool IsAvailable => CurrentBookings < MaxCapacity && IsActive;

        [NotMapped]
        public int AvailableSlots => MaxCapacity - CurrentBookings;

        [NotMapped]
        public string AvailabilityStatus =>
            !IsActive ? "Inactive" :
            CurrentBookings >= MaxCapacity ? "Full" :
            CurrentBookings == 0 ? "Available" :
            "Limited";
    }
}
