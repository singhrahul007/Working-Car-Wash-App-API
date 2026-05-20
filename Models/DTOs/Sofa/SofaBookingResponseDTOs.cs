namespace CarWash.Api.Models.DTOs.Sofa
{
    public class SofaBookingResponseDTOs
    {
        public SofaBookingDTOs? Booking { get; set; }
        public string BookingId { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}
