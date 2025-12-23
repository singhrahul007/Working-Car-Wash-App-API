using CarWash.Api.Models.Entities.AC;

namespace CarWash.Api.Models.DTOs.AC
{
    public class ACBookingDTOs
    {
        public int Id { get; set; }
        public string BookingId { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string ACType { get; set; } = string.Empty;
        public string ACBrand { get; set; } = string.Empty;
        public string ACCapacity { get; set; } = string.Empty;
        public string UsageType { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public string ScheduledTime { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<ACBookingServiceItem> ServiceItems { get; set; } = new List<ACBookingServiceItem>();
    }
}
