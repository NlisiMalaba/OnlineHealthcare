using HealthPlatform.Application.Auth;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Auth;

/// <summary>
/// Development-oriented stub: never logs the OTP value in production builds; replace with Twilio/etc.
/// </summary>
public sealed class LoggingMfaSmsSender(IHostEnvironment environment, ILogger<LoggingMfaSmsSender> logger)
    : IMfaSmsSender
{
    public Task SendOtpAsync(string phoneNumberE164, string code, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _ = phoneNumberE164;
        _ = code;
        if (environment.IsDevelopment())
        {
            logger.LogWarning("MFA SMS stub invoked (destination and OTP value are never logged).");
        }
        else
        {
            logger.LogInformation("MFA SMS dispatch requested (details omitted).");
        }

        return Task.CompletedTask;
    }
}
