using HealthPlatform.Application.NextOfKin;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Jobs;

/// <summary>
/// Hangfire recurring job that retries failed next-of-kin notification deliveries.
/// </summary>
public sealed class NextOfKinNotificationRetryJob(
    INextOfKinNotificationRetryService retryService,
    ILogger<NextOfKinNotificationRetryJob> logger)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        var processed = await retryService.ProcessDueRetriesAsync(ct);
        if (processed > 0)
        {
            logger.LogInformation("Processed {Count} next-of-kin notification retry delivery attempt(s).", processed);
        }
        else
        {
            logger.LogDebug("Next-of-kin notification retry tick — no deliveries due.");
        }
    }

    public void Run()
    {
        RunAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
}
