using CarWash.Api.DTOs;

namespace CarWash.Api.Models.DTOs.AC
{
    public class ACBookingResponseDTOs
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ACBookingDTOs? Booking { get; set; }
        public string BookingId { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}
