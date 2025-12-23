using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarWash.Api.Models.Entities.AC
{
    [Table("ACBookings")]
    public class ACBooking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string BookingId { get; set; } = GenerateBookingId();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required]
        public string CustomerAddress { get; set; } = string.Empty;
        [Required]
        [MaxLength(100)]
        public string ACType { get; set; } = string.Empty; // Split AC, Window AC, etc.

        [MaxLength(100)]
        public string ACBrand { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ACCapacity { get; set; } = string.Empty; // 1 Ton, 1.5 Ton, etc.

        [MaxLength(100)]
        public string UsageType { get; set; } = string.Empty; // Residential, Commercial

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string ScheduledTime { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "pending"; // pending, confirmed, completed, cancelled

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "pending";

        [MaxLength(500)]
        public string SpecialInstructions { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // JSON serialized list of service IDs and names
        public string SelectedServices { get; set; } = "[]";

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        // Helper method to generate booking ID
        private static string GenerateBookingId()
        {
            var date = DateTime.UtcNow.ToString("yyMMdd");
            var random = new Random().Next(1000, 9999);
            return $"AC{date}{random}";
        }

        // Not mapped properties for easy access
        [NotMapped]
        public List<ACBookingServiceItem> ServiceItems { get; set; } = new List<ACBookingServiceItem>();
    }
}
