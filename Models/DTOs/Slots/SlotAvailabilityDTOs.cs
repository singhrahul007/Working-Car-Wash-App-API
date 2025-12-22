using CarWash.Api.DTOs;

namespace CarWash.Api.Models.DTOs.Slots
{
    public class SlotAvailabilityDTOs
    {
        public DateTime Date { get; set; }
        public List<TimeSlotDto> TimeSlots { get; set; } = new List<TimeSlotDto>();
    }
    public class TimeSlotDto
    {
        public string SlotId { get; set; } // "09:00-10:00"
        public string DisplayTime { get; set; } // "09:00 AM"
        public int AvailableSlots { get; set; }
        public int TotalCapacity { get; set; }
        public bool IsAvailable { get; set; }
        public string Status { get; set; } // "available", "limited", "full"
        public string Color { get; set; } // "success", "warning", "danger"
        public decimal Price { get; set; }
    }
}
