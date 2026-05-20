using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarWash.Api.Models.Entities.Sofa
{
    [Table("SofaServices")]
    public class SofaService
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        /// <summary>Category: Basic, Deep, Premium, Leather, Sanitization, Stain</summary>
        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public int DurationInMinutes { get; set; }

        /// <summary>Human-readable duration, e.g. "90 mins"</summary>
        [MaxLength(50)]
        public string DurationDisplay { get; set; } = string.Empty;

        /// <summary>JSON-serialised list of included features</summary>
        public string Includes { get; set; } = "[]";

        public bool IsPopular { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
