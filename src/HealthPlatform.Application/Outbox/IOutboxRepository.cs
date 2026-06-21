using HealthPlatform.Domain.Events;

namespace HealthPlatform.Application.Outbox;

public interface IOutboxRepository
{
    Task EnqueueAsync(IDomainEvent domainEvent, CancellationToken ct);
}
