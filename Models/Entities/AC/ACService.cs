using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarWash.Api.Models.Entities.AC
{
    [Table("ACServices")]
    public class ACService
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty; // Maintenance, Repair, Installation
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public int DurationInMinutes { get; set; }

        public string DurationDisplay { get; set; } = string.Empty; // "1.5 hours", "2 hours"

        // JSON serialized list of included features
        public string Includes { get; set; } = "[]";

        public bool IsPopular { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<ACBooking> ACBookings { get; set; } = new List<ACBooking>();
    }
}
