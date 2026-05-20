using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarWash.Api.Models.Entities.Sofa
{
    [Table("SofaBookings")]
    public class SofaBooking
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

        /// <summary>2-Seater, 3-Seater, Sectional, L-Shaped, Recliner, Leather, Fabric</summary>
        [Required]
        [MaxLength(100)]
        public string SofaType { get; set; } = string.Empty;

        /// <summary>Number of sofas (1–5)</summary>
        public int SofaCount { get; set; } = 1;

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

        /// <summary>JSON-serialised list of SofaBookingServiceItem</summary>
        public string SelectedServices { get; set; } = "[]";

        // Navigation
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [NotMapped]
        public List<SofaBookingServiceItem> ServiceItems { get; set; } = new List<SofaBookingServiceItem>();

        // Helper
        private static string GenerateBookingId()
        {
            var date = DateTime.UtcNow.ToString("yyMMdd");
            var random = new Random().Next(1000, 9999);
            return $"SOFA{date}{random}";
        }
    }
}
