namespace HealthPlatform.Application.Auth;

/// <summary>
/// Delivers a one-time code for unrecognized-device login verification (email/SMS in production).
/// Implementations must not log or persist the plaintext code outside the delivery channel.
/// </summary>
public interface IDeviceLoginOtpNotifier
{
    Task NotifyDeviceLoginCodeAsync(string email, string oneTimeCode, CancellationToken ct);
}
