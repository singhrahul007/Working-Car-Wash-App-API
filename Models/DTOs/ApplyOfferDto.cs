// DTOs/ApplyOfferDto.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarWash.Api.DTOs
{
    public class ApplyOfferDto
    {
        [Required]
        [MaxLength(20)]
        public string OfferCode { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cart amount must be greater than 0")]
        public decimal CartAmount { get; set; }

        public List<string> ServiceCategories { get; set; } = new List<string>();
    }
}