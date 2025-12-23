using System.ComponentModel.DataAnnotations;

namespace CarWash.Api.Models.DTOs.AC
{
    public class ACBookingCreateDTOs
    {
        [Required]
        public List<int> ServiceIds { get; set; } = new List<int>();

        [Required]
        [Phone]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string CustomerAddress { get; set; } = string.Empty;

        [Required]
        public string ACType { get; set; } = string.Empty;

        public string ACBrand { get; set; } = string.Empty;
        public string ACCapacity { get; set; } = string.Empty;
        public string UsageType { get; set; } = string.Empty;
        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        public string ScheduledTime { get; set; } = string.Empty;

        public string SpecialInstructions { get; set; } = string.Empty;
    }
}
