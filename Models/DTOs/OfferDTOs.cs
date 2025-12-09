using System;
using System.Collections.Generic;

namespace CarWash.Api.DTOs
{
    public class OfferDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty; // "percentage" or "fixed"
        public decimal DiscountValue { get; set; }
        public decimal MinAmount { get; set; }
        public DateTime ValidUntil { get; set; }
        public string Category { get; set; } = string.Empty; // "all", "car-wash", "ac-service"
        public List<string> ApplicableServices { get; set; } = new();
        public string Icon { get; set; } = string.Empty;
        public string ColorCode { get; set; } = "#2196F3";
        public string Terms { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int DaysUntilExpiry { get; set; }
        public bool IsExpiringSoon => DaysUntilExpiry <= 7;
    }

    public class AppliedOfferDto
    {
        public string Code { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string Message { get; set; } = string.Empty;
    }

   
    public class OfferValidationResultDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
    }
}