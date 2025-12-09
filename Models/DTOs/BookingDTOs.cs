using CarWash.Api.DTOs.CarWash.Api.DTOs;
using System;
using System.Collections.Generic;

namespace CarWash.Api.DTOs
{
    public class    BookingDto
    {
        public string BookingId { get; set; } = string.Empty;
        public ServiceDto? Service { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public string ScheduledTime { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string VehicleType { get; set; } = string.Empty;
        public AddressDto? Address { get; set; }
        public string? ACType { get; set; }
        public string? ACBrand { get; set; }
        public string? SofaType { get; set; }
        public int? SofaCount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class BookingCreateDto
    {
        public int ServiceId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string ScheduledTime { get; set; } = string.Empty;
        public int? AddressId { get; set; }
        public AddressCreateDto? NewAddress { get; set; }
        public string VehicleType { get; set; } = "car";
        public string? ACType { get; set; }
        public string? ACBrand { get; set; }
        public string? SofaType { get; set; }
        public int? SofaCount { get; set; }
        public string? SpecialInstructions { get; set; }
        public string? AppliedOfferCode { get; set; }
        public string? PaymentMethod { get; set; }
    }

    public class CartItemDto
    {
        public int ServiceId { get; set; }
        public int Quantity { get; set; } = 1;
        public DateTime? ScheduledDate { get; set; }
        public string? ScheduledTime { get; set; }
    }

    public class CartDto
    {
        public List<CartItemDetailDto> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public AppliedOfferDto? AppliedOffer { get; set; }
    }

    public class CartItemDetailDto
    {
        public ServiceDto? Service { get; set; }
        public int Quantity { get; set; } = 1;
        public DateTime? ScheduledDate { get; set; }
        public string? ScheduledTime { get; set; }
        public bool IsAvailable { get; set; }
        public string? AvailabilityMessage { get; set; }
    }
}