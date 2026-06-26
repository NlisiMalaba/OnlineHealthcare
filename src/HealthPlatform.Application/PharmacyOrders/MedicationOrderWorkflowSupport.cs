using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Pharmacy;
using MediatR;

namespace HealthPlatform.Application.PharmacyOrders;

internal static class MedicationOrderWorkflowSupport
{
    public static async Task<Pharmacy> ResolveCurrentPharmacyAsync(
        ICurrentUserAccessor currentUser,
        IPharmacyRepository pharmacyRepository,
        CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await pharmacyRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                PharmacyErrorCodes.PharmacyNotFound,
                "Pharmacy profile was not found.");
    }

    public static async Task<MedicationOrder> LoadOrderForPharmacyAsync(
        IMedicationOrderRepository repository,
        Guid orderId,
        Guid pharmacyId,
        CancellationToken ct)
    {
        return await repository.GetByIdForPharmacyAsync(orderId, pharmacyId, ct)
            ?? throw new NotFoundException(
                PharmacyErrorCodes.OrderNotFound,
                "Medication order was not found.");
    }

    public static async Task PublishAndPersistAsync(
        MedicationOrder order,
        IMedicationOrderRepository repository,
        IOutboxRepository outboxRepository,
        IDomainEventPublisher domainEventPublisher,
        CancellationToken ct)
    {
        await repository.UpdateAsync(order, ct);

        var pendingEvents = order.DomainEvents.ToList();
        foreach (var domainEvent in pendingEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }

        order.ClearDomainEvents();
    }

    public static DomainException MapNotActionable(MedicationOrderNotActionableException exception) =>
        new(
            PharmacyErrorCodes.OrderNotActionable,
            $"Medication order cannot be updated while in status '{exception.Status}'.");
}
