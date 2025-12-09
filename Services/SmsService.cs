using CarWash.Api.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CarWash.Api.Services
{
    public class SmsService : ISmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsService> _logger;

        public SmsService(IConfiguration configuration, ILogger<SmsService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendSMSAsync(string to, string message)
        {
            try
            {
                // In production, integrate with SMS provider like Twilio, Vonage, etc.
                var provider = _configuration["SMS:Provider"] ?? "console";

                switch (provider.ToLower())
                {
                    case "twilio":
                        return await SendViaTwilio(to, message);
                    case "vonage":
                        return await SendViaVonage(to, message);
                    default:
                        // For development/testing, just log the SMS
                        _logger.LogInformation($"SMS to {to}: {message}");
                        return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send SMS to {to}");
                return false;
            }
        }

        public async Task<bool> SendOTPSMSAsync(string to, string otp, string purpose = "login")
        {
            var purposeText = purpose switch
            {
                "signup" => "registration",
                "reset" => "password reset",
                _ => "login"
            };

            var message = $"Your CarWash {purposeText} OTP is: {otp}. Valid for 10 minutes.";

            return await SendSMSAsync(to, message);
        }

        // ADD THIS MISSING METHOD
        public async Task SendOTPSmsAsync(string mobileNumber, string otp, string flow)
        {
            try
            {
                // Format the message based on flow
                var message = flow.ToLower() switch
                {
                    "login" => $"Your login OTP is {otp}. Valid for 10 minutes.",
                    "signup" => $"Your verification OTP is {otp}. Valid for 10 minutes.",
                    "reset" => $"Your password reset OTP is {otp}. Valid for 10 minutes.",
                    _ => $"Your OTP is {otp}. Valid for 10 minutes."
                };

                // Use the existing SendSMSAsync method
                var success = await SendSMSAsync(mobileNumber, message);

                if (!success)
                {
                    throw new Exception("Failed to send OTP SMS");
                }

                _logger.LogInformation($"OTP SMS sent to {mobileNumber} for flow: {flow}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send OTP SMS to {mobileNumber}");
                throw;
            }
        }

        private async Task<bool> SendViaTwilio(string to, string message)
        {
            // Twilio implementation
            var accountSid = _configuration["Twilio:AccountSid"];
            var authToken = _configuration["Twilio:AuthToken"];
            var fromNumber = _configuration["Twilio:FromNumber"];

            if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken))
            {
                _logger.LogWarning("Twilio credentials not configured. Logging SMS instead.");
                _logger.LogInformation($"Twilio SMS to {to}: {message}");
                return true;
            }

            // Uncomment when you have Twilio package installed
            /*
            TwilioClient.Init(accountSid, authToken);
            
            var smsMessage = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(fromNumber),
                to: new PhoneNumber(to)
            );
            
            return smsMessage.Status != MessageResource.StatusEnum.Failed;
            */

            // For now, just log
            _logger.LogInformation($"Twilio SMS would be sent to {to}: {message}");
            return true;
        }

        private async Task<bool> SendViaVonage(string to, string message)
        {
            // Vonage/Nexmo implementation
            var apiKey = _configuration["Vonage:ApiKey"];
            var apiSecret = _configuration["Vonage:ApiSecret"];
            var fromNumber = _configuration["Vonage:FromNumber"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                _logger.LogWarning("Vonage credentials not configured. Logging SMS instead.");
                _logger.LogInformation($"Vonage SMS to {to}: {message}");
                return true;
            }

            // Uncomment when you have Vonage package installed
            /*
            var client = new Client(creds: new Nexmo.Api.Request.Credentials
            {
                ApiKey = apiKey,
                ApiSecret = apiSecret
            });
            
            var results = client.SMS.Send(new SMS.SMSRequest
            {
                from = fromNumber,
                to = to,
                text = message
            });
            
            return results.messages[0].status == "0";
            */

            // For now, just log
            _logger.LogInformation($"Vonage SMS would be sent to {to}: {message}");
            return true;
        }
    }
}