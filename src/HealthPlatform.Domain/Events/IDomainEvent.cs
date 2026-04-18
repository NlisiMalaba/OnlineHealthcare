namespace HealthPlatform.Domain.Events;

/// <summary>
/// Marker for domain events raised by aggregates (persisted via outbox for reliable dispatch).
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAtUtc { get; }
}
