using System.ComponentModel.DataAnnotations;

namespace CarWash.Api.Models.DTOs.Sofa
{
    public class SofaBookingCreateDTOs
    {
        [Required]
        public List<int> ServiceIds { get; set; } = new List<int>();

        [Required]
        [Phone]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string CustomerAddress { get; set; } = string.Empty;

        /// <summary>2-Seater, 3-Seater, Sectional, L-Shaped, Recliner, Leather, Fabric</summary>
        [Required]
        [MaxLength(100)]
        public string SofaType { get; set; } = string.Empty;

        /// <summary>Number of sofas (1–5)</summary>
        [Range(1, 5)]
        public int SofaCount { get; set; } = 1;

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string ScheduledTime { get; set; } = string.Empty;

        [MaxLength(500)]
        public string SpecialInstructions { get; set; } = string.Empty;
    }
}
