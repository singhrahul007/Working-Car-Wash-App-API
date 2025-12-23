namespace CarWash.Api.Models.Entities.AC
{
    public class ACBookingServiceItem
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Duration { get; set; } = string.Empty;
        public string Includes { get; set; } = string.Empty;
    }
}
