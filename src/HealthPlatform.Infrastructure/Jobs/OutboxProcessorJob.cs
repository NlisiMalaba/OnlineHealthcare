using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Jobs;

/// <summary>
/// Placeholder for outbox domain-event dispatch (Hangfire recurring job).
/// </summary>
public sealed class OutboxProcessorJob(ILogger<OutboxProcessorJob> logger)
{
    public void Run()
    {
        logger.LogInformation("Outbox processor tick — no pending outbox rows to process (scaffold).");
    }
}
