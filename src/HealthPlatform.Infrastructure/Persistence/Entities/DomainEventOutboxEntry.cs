namespace HealthPlatform.Infrastructure.Persistence.Entities;

public sealed class DomainEventOutboxEntry
{
    public Guid Id { get; set; }

    public Guid EventId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public DateTime OccurredAtUtc { get; set; }

    public DateTime? ProcessedAtUtc { get; set; }
}
