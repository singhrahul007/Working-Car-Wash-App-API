
using System.ComponentModel.DataAnnotations;

namespace CarWash.Api.Models.DTOs
{
    public class VerifyOtpDto
    {
        [Required]
        public string Type { get; set; } = "mobile"; // "mobile" or "email"

        [Required]
        public string Value { get; set; } = string.Empty; // Mobile number or email

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string OTP { get; set; } = string.Empty;

        [Required]
        public string Flow { get; set; } = "login"; // "login", "signup", "reset"
    }
}