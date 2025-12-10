using System.ComponentModel.DataAnnotations;

namespace CarWash.Api.Models.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        public string LoginType { get; set; } = "mobile"; // "mobile" or "email"

        [Phone(ErrorMessage = "Invalid mobile number")]
        [MaxLength(10)]
        public string? MobileNumber { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }

        public bool RememberMe { get; set; }
        public string? DeviceId { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
    }

    public class SocialLoginRequestDto
    {
        [Required]
        public string Provider { get; set; } = "google"; // "google", "facebook", "apple"

        [Required]
        public string Token { get; set; } = string.Empty;

        public string? DeviceId { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
    }

    public class OTPRequestDto
    {
        [Required]
        public string Type { get; set; } = "mobile"; // "mobile" or "email"

        [Required]
        public string Value { get; set; } = string.Empty; // Mobile number or email

        [Required]
        public string Flow { get; set; } = "login"; // "login", "signup", "reset"

        public string? DeviceId { get; set; }
        public string? IpAddress { get; set; }
    }

    public class VerifyOTPRequestDto
    {
        [Required]
        public string Type { get; set; } = "mobile";

        [Required]
        public string Value { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
        public string OTP { get; set; } = string.Empty;

        [Required]
        public string Flow { get; set; } = "login";
       
    }


    public class ForgotPasswordRequestDto
    {
        [Required]
        public string Type { get; set; } = "email"; // "email" or "mobile"

        [Required]
        public string Value { get; set; } = string.Empty;
    }

    public class ResetPasswordRequestDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}