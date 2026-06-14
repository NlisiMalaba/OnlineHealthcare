using HealthPlatform.Domain.Events;

namespace HealthPlatform.Application.Outbox;

public interface IDomainEventPublisher
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct);
}
