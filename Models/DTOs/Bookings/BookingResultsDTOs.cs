namespace CarWash.Api.Models.DTOs.Bookings
{
    public class BookingResultsDTOs
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int BookingId { get; set; }
        public string BookingReference { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
