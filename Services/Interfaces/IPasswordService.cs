namespace CarWash.Api.Interfaces
{
    public interface IPasswordService
    {
        string HashPassword(string password, out byte[] salt);
        bool VerifyPassword(string password, string hash, byte[] salt);
        bool IsStrongPassword(string password);
    }
}