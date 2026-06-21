namespace HealthPlatform.Application.Auth;

/// <summary>
/// Provider names aligned with ASP.NET Core Identity token providers (Authenticator + Phone/SMS OTP).
/// </summary>
public static class TwoFactorProviders
{
    public const string Authenticator = "Authenticator";
    public const string Phone = "Phone";
}
