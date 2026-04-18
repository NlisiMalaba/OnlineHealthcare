using HealthPlatform.Application.Outbox;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Outbox;

/// <summary>
/// Placeholder until outbox table + MediatR dispatch wiring exists (Hangfire job calls this).
/// </summary>
public sealed class NoOpOutboxDomainEventDispatcher(ILogger<NoOpOutboxDomainEventDispatcher> logger)
    : IOutboxDomainEventDispatcher
{
    public Task<int> DispatchPendingAsync(CancellationToken ct)
    {
        logger.LogDebug("Outbox dispatch tick — no persistence layer registered yet.");
        return Task.FromResult(0);
    }
}
