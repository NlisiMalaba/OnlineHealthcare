using System.Text.Json;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Events;
using HealthPlatform.Domain.Identity.Events;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Outbox;

public sealed class OutboxDomainEventDispatcher(
    ApplicationDbContext db,
    IDomainEventPublisher publisher,
    ILogger<OutboxDomainEventDispatcher> logger) : IOutboxDomainEventDispatcher
{
    private const int BatchSize = 50;

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<int> DispatchPendingAsync(CancellationToken ct)
    {
        var pending = await db.DomainEventOutbox
            .Where(x => x.ProcessedAtUtc == null)
            .OrderBy(x => x.OccurredAtUtc)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (pending.Count == 0)
        {
            return 0;
        }

        var dispatched = 0;
        foreach (var row in pending)
        {
            ct.ThrowIfCancellationRequested();
            var domainEvent = Deserialize(row.EventType, row.Payload);
            if (domainEvent is null)
            {
                logger.LogError("Skipping outbox row {OutboxId} with unknown event type {EventType}.", row.Id, row.EventType);
                row.ProcessedAtUtc = DateTime.UtcNow;
                continue;
            }

            await publisher.PublishAsync(domainEvent, ct);
            row.ProcessedAtUtc = DateTime.UtcNow;
            dispatched++;
        }

        await db.SaveChangesAsync(ct);
        return dispatched;
    }

    private static IDomainEvent? Deserialize(string eventType, string payload) =>
        eventType switch
        {
            "HealthPlatform.Domain.Identity.Events.AccountLockedDomainEvent" =>
                JsonSerializer.Deserialize<AccountLockedDomainEvent>(payload, SerializerOptions),
            "HealthPlatform.Domain.Identity.Events.PatientRegisteredDomainEvent" =>
                JsonSerializer.Deserialize<PatientRegisteredDomainEvent>(payload, SerializerOptions),
            _ => null
        };
}
