using System.ComponentModel.DataAnnotations;

namespace CarWash.Api.Models.DTOs.Bookings
{
    public class BookingRequestDTOs
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string TimeSlot { get; set; } // "09:00-10:00"

        [Range(1, 10)]
        public int Quantity { get; set; } = 1;

        [StringLength(1000)]
        public string Notes { get; set; }
        public string PromoCode { get; set; }
    }
}
