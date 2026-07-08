using HealthPlatform.Application.Notifications;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Jobs;

/// <summary>
/// Hangfire job that processes critical notification SMS fallbacks when push delivery fails.
/// </summary>
public sealed class CriticalNotificationSmsFallbackJob(
    ICriticalNotificationSmsFallbackService fallbackService,
    ILogger<CriticalNotificationSmsFallbackJob> logger)
{
    public async Task ProcessFallbackAsync(Guid fallbackId, CancellationToken ct = default)
    {
        var succeeded = await fallbackService.ProcessAsync(fallbackId, ct);
        if (succeeded)
        {
            logger.LogInformation(
                "Critical SMS fallback {FallbackId} delivered successfully.",
                fallbackId);
        }
    }

    public void ProcessFallback(Guid fallbackId) =>
        ProcessFallbackAsync(fallbackId, CancellationToken.None).GetAwaiter().GetResult();

    public async Task RunAsync(CancellationToken ct = default)
    {
        var processed = await fallbackService.ProcessDueAsync(ct);
        if (processed > 0)
        {
            logger.LogInformation(
                "Processed {Count} critical notification SMS fallback attempt(s).",
                processed);
        }
        else
        {
            logger.LogDebug("Critical notification SMS fallback tick — no deliveries due.");
        }
    }

    public void Run() => RunAsync(CancellationToken.None).GetAwaiter().GetResult();
}
