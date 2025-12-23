// Entities/Booking.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarWash.Api.Models.Entities
{
    public class Booking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // Auto-incrementing integer primary key

        [Required]
        [MaxLength(20)]
        public string BookingId { get; set; } = GenerateBookingId();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int ServiceId { get; set; }
        public int SlotId { get; set; }

        public int? AddressId { get; set; }

        [Required]
        [MaxLength(50)]
        public string VehicleType { get; set; } = "car";

        [MaxLength(100)]
        public string? ACType { get; set; }

        [MaxLength(100)]
        public string? ACBrand { get; set; }

        [MaxLength(100)]
        public string? SofaType { get; set; }

        public int? SofaCount { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string? ScheduledTime { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "pending";

        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "pending";

        [MaxLength(100)]
        public string? PaymentMethod { get; set; }

        [MaxLength(20)]
        public string? AppliedOfferCode { get; set; }

        [MaxLength(500)]
        public string? SpecialInstructions { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; } = null!;
        [ForeignKey("SlotId")]
        public virtual Slot Slot { get; set; }

        [ForeignKey("AddressId")]
        public virtual Address? Address { get; set; }

        // Helper method to generate booking ID
        private static string GenerateBookingId()
        {
            var date = DateTime.UtcNow.ToString("yyMMdd");
            var random = new Random().Next(1000, 9999);
            return $"CW{date}{random}";
        }
    }
}