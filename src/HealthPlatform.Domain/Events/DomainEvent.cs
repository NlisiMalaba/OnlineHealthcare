namespace HealthPlatform.Domain.Events;

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();
    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;
}
