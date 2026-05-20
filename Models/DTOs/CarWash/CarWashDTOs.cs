using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CarWash.Api.Models.Entities.CarWash;

namespace CarWash.Api.Models.DTOs.CarWash
{
    public class CarWashServiceDTOs
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

    public class CarWashServiceFilterDTOs
    {
        public string? Category { get; set; }
        public bool? IsPopular { get; set; }
    }

    public class CarWashBookingCreateDTOs
    {
        [Required]
        public List<int> ServiceIds { get; set; } = new List<int>();

        [Required]
        [Phone]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string CustomerAddress { get; set; } = string.Empty;

        /// <summary>Hatchback, Sedan, SUV, Luxury</summary>
        [Required]
        [MaxLength(100)]
        public string VehicleSize { get; set; } = string.Empty;

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string ScheduledTime { get; set; } = string.Empty;

        [MaxLength(500)]
        public string SpecialInstructions { get; set; } = string.Empty;
    }

    public class CarWashBookingDTOs
    {
        public int Id { get; set; }
        public string BookingId { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string VehicleSize { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public string ScheduledTime { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string SpecialInstructions { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<CarWashBookingServiceItem> ServiceItems { get; set; } = new List<CarWashBookingServiceItem>();
    }

    public class CarWashBookingResponseDTOs
    {
        public CarWashBookingDTOs Booking { get; set; } = null!;
        public string BookingId { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}
