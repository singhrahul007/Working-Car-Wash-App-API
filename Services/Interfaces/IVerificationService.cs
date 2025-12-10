namespace CarWash.Api.Services.Interfaces
{
    public interface IVerificationService
    {
        Task<string> GenerateEmailVerificationToken(string email);
        Task<string> GenerateMobileVerificationCode(string mobileNumber);
        Task<bool> VerifyEmailToken(string email, string token);
        Task<bool> VerifyMobileCode(string mobileNumber, string code);
    }
}
