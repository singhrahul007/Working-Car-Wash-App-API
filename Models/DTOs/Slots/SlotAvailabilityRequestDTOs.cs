using System.ComponentModel.DataAnnotations;

namespace CarWash.Api.Models.DTOs.Slots
{
    public class SlotAvailabilityRequestDTOs
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int SlotId { get; set; }

        [Required]
        [Range(1, 10)]
        public int Quantity { get; set; } = 1;
    }
}
