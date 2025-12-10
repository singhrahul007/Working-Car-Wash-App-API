namespace CarWash.Api.Services.Interfaces
{
    public interface IVerificationService
    {
        Task<string> GenerateEmailVerificationToken(string email);
        Task<string> GenerateMobileVerificationCode(string MobileNumber);
        Task<bool> VerifyEmailToken(string email, string token);
        Task<bool> VerifyMobileCode(string MobileNumber, string code);
    }
}
