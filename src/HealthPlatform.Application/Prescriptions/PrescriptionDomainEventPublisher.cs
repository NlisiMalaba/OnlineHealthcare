using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Prescriptions;

namespace HealthPlatform.Application.Prescriptions;

public interface IPrescriptionDomainEventPublisher
{
    Task PublishPendingAsync(Prescription prescription, CancellationToken ct);
}

public sealed class PrescriptionDomainEventPublisher(
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher) : IPrescriptionDomainEventPublisher
{
    public async Task PublishPendingAsync(Prescription prescription, CancellationToken ct)
    {
        var pendingEvents = prescription.DomainEvents.ToList();
        foreach (var domainEvent in pendingEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }

        prescription.ClearDomainEvents();
    }
}
