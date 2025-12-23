using System.ComponentModel.DataAnnotations;

namespace CarWash.Api.Models.DTOs.Bookings
{
    public class BookingRequestDTOs
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Required]
        public int SlotId { get; set; } // Keep this

     
        public int? AddressId { get; set; }

        [Required]
        [Range(0.01, 10000)]
        public decimal Subtotal { get; set; }

        public decimal? DiscountAmount { get; set; }
        public decimal? TaxAmount { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        public string? VehicleType { get; set; }
        public string? SpecialInstructions { get; set; }
        public string? AppliedOfferCode { get; set; }
        public string? PaymentMethod { get; set; }

        // Keep Notes if needed (map to SpecialInstructions)
        [StringLength(1000)]
        public string? Notes { get; set; }
        public string? PromoCode { get; set; }
    }

}
