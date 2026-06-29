using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Pharmacy;
using MediatR;

namespace HealthPlatform.Application.PharmacyOrders.MarkMedicationOrderDispatched;

public sealed class MarkMedicationOrderDispatchedCommandHandler(
    ICurrentUserAccessor currentUser,
    IPharmacyRepository pharmacyRepository,
    IMedicationOrderRepository medicationOrderRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    TimeProvider timeProvider)
    : IRequestHandler<MarkMedicationOrderDispatchedCommand, MedicationOrderDto>
{
    public async Task<MedicationOrderDto> Handle(MarkMedicationOrderDispatchedCommand request, CancellationToken ct)
    {
        var pharmacy = await MedicationOrderWorkflowSupport.ResolveCurrentPharmacyAsync(
            currentUser,
            pharmacyRepository,
            ct);

        var order = await MedicationOrderWorkflowSupport.LoadOrderForPharmacyAsync(
            medicationOrderRepository,
            request.OrderId,
            pharmacy.Id,
            ct);

        try
        {
            order.MarkDispatched(timeProvider.GetUtcNow().UtcDateTime);
        }
        catch (MedicationOrderNotActionableException exception)
        {
            throw MedicationOrderWorkflowSupport.MapNotActionable(exception);
        }

        await MedicationOrderWorkflowSupport.PublishAndPersistAsync(
            order,
            medicationOrderRepository,
            outboxRepository,
            domainEventPublisher,
            ct);

        return order.ToDto();
    }
}
