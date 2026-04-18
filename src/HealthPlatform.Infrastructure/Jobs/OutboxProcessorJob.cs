using HealthPlatform.Application.Outbox;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Jobs;

/// <summary>
/// Hangfire recurring job that drains the transactional outbox via <see cref="IOutboxDomainEventDispatcher"/>.
/// </summary>
public sealed class OutboxProcessorJob(
    IOutboxDomainEventDispatcher dispatcher,
    ILogger<OutboxProcessorJob> logger)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        var processed = await dispatcher.DispatchPendingAsync(ct);
        if (processed > 0)
        {
            logger.LogInformation("Outbox processor dispatched {Count} event(s).", processed);
        }
        else
        {
            logger.LogDebug("Outbox processor tick — nothing to dispatch.");
        }
    }

    /// <summary>
    /// Hangfire entry point (void) — avoids exposing <see cref="CancellationToken"/> through Hangfire reflection.
    /// </summary>
    public void Run()
    {
        RunAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
}
