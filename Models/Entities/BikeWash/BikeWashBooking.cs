using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarWash.Api.Models.Entities.BikeWash
{
    [Table("BikeWashBookings")]
    public class BikeWashBooking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(25)]
        public string BookingId { get; set; } = GenerateBookingId();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(15)]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string CustomerAddress { get; set; } = string.Empty;

        /// <summary>Bike type: Standard, Cruiser, Sports</summary>
        [Required]
        [MaxLength(100)]
        public string BikeType { get; set; } = string.Empty;

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string ScheduledTime { get; set; } = string.Empty;

        /// <summary>pending | confirmed | completed | cancelled</summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "pending";

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "pending";

        [MaxLength(500)]
        public string SpecialInstructions { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        /// <summary>JSON-serialised list of BikeWashBookingServiceItem</summary>
        public string SelectedServices { get; set; } = "[]";

        // Navigation
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [NotMapped]
        public List<BikeWashBookingServiceItem> ServiceItems { get; set; } = new List<BikeWashBookingServiceItem>();

        // Helper
        private static string GenerateBookingId()
        {
            var date = DateTime.UtcNow.ToString("yyMMdd");
            var random = new Random().Next(1000, 9999);
            return $"BIKE{date}{random}";
        }
    }
}
