using HealthPlatform.Application.Auth;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Auth;

public sealed class LoggingDeviceLoginOtpNotifier(
    IHostEnvironment environment,
    IOptions<DeviceLoginOptions> options,
    ILogger<LoggingDeviceLoginOtpNotifier> logger) : IDeviceLoginOtpNotifier
{
    public Task NotifyDeviceLoginCodeAsync(string email, string oneTimeCode, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (environment.IsDevelopment() && options.Value.LogOneTimeCodeInDevelopment)
        {
            logger.LogWarning(
                "Device login verification code for {Email} (development diagnostics only): {Code}",
                email,
                oneTimeCode);
        }
        else
        {
            logger.LogInformation("Device login OTP dispatch requested for {Email} (code omitted).", email);
        }

        return Task.CompletedTask;
    }
}
