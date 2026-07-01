using HealthPlatform.Application.Wellness;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Jobs;

/// <summary>
/// Hangfire recurring job that records missed adherence events for unconfirmed doses.
/// </summary>
public sealed class MissedDoseDetectionJob(
    IMissedDoseDetectionDispatcher missedDoseDetectionDispatcher,
    ILogger<MissedDoseDetectionJob> logger)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        var recorded = await missedDoseDetectionDispatcher.RecordMissedDosesAsync(ct);
        if (recorded > 0)
        {
            logger.LogInformation("Missed dose detection recorded {Count} adherence event(s).", recorded);
        }
        else
        {
            logger.LogDebug("Missed dose detection tick — no missed doses to record.");
        }
    }

    public void Run()
    {
        RunAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
}
