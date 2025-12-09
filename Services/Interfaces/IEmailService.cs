namespace CarWash.Api.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task<bool> SendOTPEmailAsync(string to, string otp, string purpose = "login");
        Task<bool> SendWelcomeEmailAsync(string to, string name);
        Task<bool> SendPasswordResetEmailAsync(string to, string resetToken);
        Task SendBookingConfirmationAsync(string toEmail, string bookingId, string serviceName, DateTime scheduledDate);
    }
}
