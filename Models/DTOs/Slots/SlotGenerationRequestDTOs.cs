using System.ComponentModel.DataAnnotations;

namespace CarWash.Api.Models.DTOs.Slots
{
    public class SlotGenerationRequestDTOs
    {
        [Required]
        public int ServiceId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
}
