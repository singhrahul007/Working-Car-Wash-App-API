using System;
using System.ComponentModel.DataAnnotations;

namespace CarWash.Api.DTOs
{
  

    public class RegisterRequestDto
    {
        [EmailAddress]
        public string? Email { get; set; }
        public string? MobileNumber { get; set; }

        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
        public bool AcceptTerms { get; set; }
    }



   


}