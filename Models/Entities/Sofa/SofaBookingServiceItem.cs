namespace CarWash.Api.Models.Entities.Sofa
{
    /// <summary>
    /// Value object (not a DB table) stored as JSON inside SofaBooking.SelectedServices.
    /// </summary>
    public class SofaBookingServiceItem
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Duration { get; set; } = string.Empty;
        public string Includes { get; set; } = string.Empty;
    }
}
