using System.ComponentModel.DataAnnotations;

namespace CarWash.Api.Models.DTOs.Slots
{
    public class CreateSlotDTOs
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$",
            ErrorMessage = "Time must be in HH:mm format")]
        public string StartTime { get; set; }

        [Required]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$",
            ErrorMessage = "Time must be in HH:mm format")]
        public string EndTime { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Range(1, 100)]
        public int MaxCapacity { get; set; } = 5;

    }
}
