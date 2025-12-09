using CarWash.Api.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CarWash.Api.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = _configuration.GetValue<int>("Email:SmtpPort", 587);
                var smtpUsername = _configuration["Email:SmtpUsername"];
                var smtpPassword = _configuration["Email:SmtpPassword"];
                var fromEmail = _configuration["Email:FromEmail"] ?? "noreply@carwash.com";

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, "CarWash Service"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                message.To.Add(to);

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent to {to}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}");

                // For development/testing, log the email instead
                _logger.LogInformation($"DEV MODE: Would send email to {to}");
                _logger.LogInformation($"Subject: {subject}");
                _logger.LogInformation($"Body: {body}");

                return false;
            }
        }

        public async Task<bool> SendOTPEmailAsync(string to, string otp, string purpose = "login")
        {
            var subject = purpose switch
            {
                "signup" => "Verify Your Email - CarWash",
                "reset" => "Reset Your Password - CarWash",
                _ => "Your Login OTP - CarWash"
            };

            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2196F3;'>CarWash Service</h2>
                    <p>Your OTP for {purpose} is:</p>
                    <div style='background-color: #f5f5f5; padding: 20px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 5px; margin: 20px 0;'>
                        {otp}
                    </div>
                    <p>This OTP is valid for 10 minutes.</p>
                    <p>If you didn't request this, please ignore this email.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                    <p style='color: #666; font-size: 12px;'>This is an automated message, please do not reply.</p>
                </div>";

            return await SendEmailAsync(to, subject, body);
        }

        public async Task<bool> SendWelcomeEmailAsync(string to, string name)
        {
            var subject = "Welcome to CarWash!";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2196F3;'>Welcome to CarWash, {name}!</h2>
                    <p>Thank you for registering with CarWash. We're excited to have you on board!</p>
                    <p>With our app, you can:</p>
                    <ul>
                        <li>Book car wash services online</li>
                        <li>Track your service history</li>
                        <li>Receive special offers and discounts</li>
                        <li>Get notifications about your bookings</li>
                    </ul>
                    <p>If you have any questions, feel free to contact our support team.</p>
                    <p>Happy washing!</p>
                    <p>The CarWash Team</p>
                </div>";

            return await SendEmailAsync(to, subject, body);
        }

        public async Task<bool> SendPasswordResetEmailAsync(string to, string resetToken)
        {
            var resetUrl = $"{_configuration["App:BaseUrl"]}/reset-password?token={resetToken}";

            var subject = "Reset Your CarWash Password";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2196F3;'>Password Reset Request</h2>
                    <p>You requested to reset your CarWash password.</p>
                    <p>Click the link below to reset your password:</p>
                    <p><a href='{resetUrl}' style='background-color: #2196F3; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>Reset Password</a></p>
                    <p>Or copy and paste this link in your browser:<br/>{resetUrl}</p>
                    <p>This link will expire in 1 hour.</p>
                    <p>If you didn't request a password reset, please ignore this email.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                    <p style='color: #666; font-size: 12px;'>This is an automated message, please do not reply.</p>
                </div>";

            return await SendEmailAsync(to, subject, body);
        }

        // ADD THIS MISSING METHOD
        public async Task SendBookingConfirmationAsync(string toEmail, string bookingId, string serviceName, DateTime scheduledDate)
        {
            try
            {
                var subject = $"Booking Confirmation - #{bookingId}";
                var formattedDate = scheduledDate.ToString("dddd, MMMM dd, yyyy 'at' hh:mm tt");

                var body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #2196F3;'>Booking Confirmed!</h2>
                        <p>Thank you for booking with CarWash. Your booking has been confirmed.</p>
                        
                        <div style='background-color: #f9f9f9; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                            <h3 style='margin-top: 0;'>Booking Details:</h3>
                            <p><strong>Booking ID:</strong> {bookingId}</p>
                            <p><strong>Service:</strong> {serviceName}</p>
                            <p><strong>Scheduled Date:</strong> {formattedDate}</p>
                            <p><strong>Status:</strong> Confirmed</p>
                        </div>
                        
                        <p><strong>Important Notes:</strong></p>
                        <ul>
                            <li>Please arrive 10 minutes before your scheduled time</li>
                            <li>Make sure your vehicle is accessible</li>
                            <li>Remove all personal belongings from your vehicle</li>
                            <li>If you need to cancel or reschedule, please do so at least 2 hours in advance</li>
                        </ul>
                        
                        <p>You can track your booking status in the CarWash app.</p>
                        
                        <p>If you have any questions, contact our support team at support@carwash.com</p>
                        
                        <p>Thank you for choosing CarWash!</p>
                        <p>The CarWash Team</p>
                    </div>";

                var success = await SendEmailAsync(toEmail, subject, body);

                if (!success)
                {
                    throw new Exception("Failed to send booking confirmation email");
                }

                _logger.LogInformation($"Booking confirmation email sent to {toEmail} for booking {bookingId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send booking confirmation email to {toEmail}");
                throw;
            }
        }
    }
}