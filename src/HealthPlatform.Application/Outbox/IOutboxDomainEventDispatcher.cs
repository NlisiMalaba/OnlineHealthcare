namespace HealthPlatform.Application.Outbox;

/// <summary>
/// Dispatches domain events from the transactional outbox (at-least-once; handlers must be idempotent).
/// </summary>
public interface IOutboxDomainEventDispatcher
{
    /// <summary>
    /// Processes a batch of pending outbox rows and returns how many were dispatched.
    /// </summary>
    Task<int> DispatchPendingAsync(CancellationToken ct);
}
