using HealthPlatform.Application.Auth;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingDeviceLoginOtpNotifier : IDeviceLoginOtpNotifier
{
    public string? LastEmail { get; private set; }

    public string? LastCode { get; private set; }

    public Task NotifyDeviceLoginCodeAsync(string email, string oneTimeCode, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        LastEmail = email;
        LastCode = oneTimeCode;
        return Task.CompletedTask;
    }
}
