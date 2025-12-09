namespace CarWash.Api.Models.Enums
{
    public enum LoginType
    {
        Mobile,
        Email
    }

    public enum OTPType
    {
        Login,
        Signup,
        Reset,
        Verify
    }

    public enum OTPPurpose
    {
        Mobile,
        Email,
        TwoFactor
    }

    public enum SocialProvider
    {
        Google,
        Facebook,
        Apple
    }
}