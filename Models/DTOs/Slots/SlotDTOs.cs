namespace CarWash.Api.Models.DTOs.Slots
{
    public class SlotDTOs
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string Category { get; set; }
        public int MaxCapacity { get; set; }
        public int CurrentBookings { get; set; }
        public int AvailableSlots { get; set; }
        public bool IsAvailable { get; set; }
        public string AvailabilityStatus { get; set; } // "Available", "Limited", "Full"
        public string ColorCode { get; set; } // "green", "yellow", "red"
        public DateTime CreatedAt { get; set; }
    }
}
