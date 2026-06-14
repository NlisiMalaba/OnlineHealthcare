using System.Text.Json;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Events;
using HealthPlatform.Infrastructure.Persistence;
using HealthPlatform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Outbox;

public sealed class OutboxRepository(ApplicationDbContext db) : IOutboxRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task EnqueueAsync(IDomainEvent domainEvent, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        var eventType = domainEvent.GetType().FullName
            ?? throw new InvalidOperationException("Domain event type must have a full name.");
        var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), SerializerOptions);

        db.DomainEventOutbox.Add(
            new DomainEventOutboxEntry
            {
                Id = Guid.CreateVersion7(),
                EventId = domainEvent.EventId,
                EventType = eventType,
                Payload = payload,
                OccurredAtUtc = domainEvent.OccurredAtUtc,
                ProcessedAtUtc = null
            });

        await db.SaveChangesAsync(ct);
    }
}
