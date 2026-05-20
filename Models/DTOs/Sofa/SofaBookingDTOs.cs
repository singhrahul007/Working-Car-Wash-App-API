using CarWash.Api.Models.Entities.Sofa;

namespace CarWash.Api.Models.DTOs.Sofa
{
    public class SofaBookingDTOs
    {
        public int Id { get; set; }
        public string BookingId { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string SofaType { get; set; } = string.Empty;
        public int SofaCount { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string ScheduledTime { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string SpecialInstructions { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<SofaBookingServiceItem> ServiceItems { get; set; } = new List<SofaBookingServiceItem>();
    }
}
