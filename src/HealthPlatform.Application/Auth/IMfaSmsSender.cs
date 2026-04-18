namespace HealthPlatform.Application.Auth;

/// <summary>
/// Sends SMS OTP codes for MFA (implemented in Infrastructure; never log PHI in implementations).
/// </summary>
public interface IMfaSmsSender
{
    Task SendOtpAsync(string phoneNumberE164, string code, CancellationToken ct);
}
