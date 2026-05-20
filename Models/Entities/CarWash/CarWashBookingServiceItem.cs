namespace CarWash.Api.Models.Entities.CarWash
{
    /// <summary>
    /// Value object stored as JSON inside CarWashBooking.SelectedServices.
    /// </summary>
    public class CarWashBookingServiceItem
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Duration { get; set; } = string.Empty;
        public string Includes { get; set; } = string.Empty;
    }
}
