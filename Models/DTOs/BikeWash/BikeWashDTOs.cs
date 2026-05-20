using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CarWash.Api.Models.Entities.BikeWash;

namespace CarWash.Api.Models.DTOs.BikeWash
{
    public class BikeWashServiceDTOs
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DurationInMinutes { get; set; }
        public string DurationDisplay { get; set; } = string.Empty;
        public List<string> Includes { get; set; } = new List<string>();
        public bool IsPopular { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class BikeWashServiceFilterDTOs
    {
        public string? Category { get; set; }
        public bool? IsPopular { get; set; }
    }

    public class BikeWashBookingCreateDTOs
    {
        [Required]
        public List<int> ServiceIds { get; set; } = new List<int>();

        [Required]
        [Phone]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string CustomerAddress { get; set; } = string.Empty;

        /// <summary>Standard, Cruiser, Sports</summary>
        [Required]
        [MaxLength(100)]
        public string BikeType { get; set; } = string.Empty;

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string ScheduledTime { get; set; } = string.Empty;

        [MaxLength(500)]
        public string SpecialInstructions { get; set; } = string.Empty;
    }

    public class BikeWashBookingDTOs
    {
        public int Id { get; set; }
        public string BookingId { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string BikeType { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public string ScheduledTime { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string SpecialInstructions { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<BikeWashBookingServiceItem> ServiceItems { get; set; } = new List<BikeWashBookingServiceItem>();
    }

    public class BikeWashBookingResponseDTOs
    {
        public BikeWashBookingDTOs Booking { get; set; } = null!;
        public string BookingId { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}
