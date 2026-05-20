namespace CarWash.Api.Models.Entities.BikeWash
{
    /// <summary>
    /// Value object stored as JSON inside BikeWashBooking.SelectedServices.
    /// </summary>
    public class BikeWashBookingServiceItem
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Duration { get; set; } = string.Empty;
        public string Includes { get; set; } = string.Empty;
    }
}
