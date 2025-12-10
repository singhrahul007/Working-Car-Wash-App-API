namespace CarWash.Api.Services.Interfaces
{
    public interface ISmsService
    {
        Task<bool> SendSMSAsync(string to, string message);
        Task<bool> SendOTPSMSAsync(string to, string otp, string purpose = "login");
        Task SendOTPSmsAsync(string MobileNumber, string otp, string flow);
    }
}