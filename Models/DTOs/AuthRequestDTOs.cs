using System;
using System.ComponentModel.DataAnnotations;

namespace CarWash.Api.DTOs
{
  

    public class RegisterRequestDto
    {
        [EmailAddress]
        public string? Email { get; set; }
        [MaxLength(10)]
        public string MobileNumber { get; set; }

        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
        public bool AcceptTerms { get; set; }
    }
    public class VerifyEmailRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;
    }
    public class VerifyMobileRequestDto
    {
        [Required]
        [Phone]
        [MaxLength(10)]
        public string MobileNumber { get; set; } 

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Code { get; set; } = string.Empty;
    }
    public class SendVerificationRequestDto
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        [MaxLength(10)]
        public string? MobileNumber { get; set; }
    }





}