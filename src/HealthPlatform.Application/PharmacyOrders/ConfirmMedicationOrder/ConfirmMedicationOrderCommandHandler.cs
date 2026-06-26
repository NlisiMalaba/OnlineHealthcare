using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Pharmacy;
using MediatR;

namespace HealthPlatform.Application.PharmacyOrders.ConfirmMedicationOrder;

public sealed class ConfirmMedicationOrderCommandHandler(
    ICurrentUserAccessor currentUser,
    IPharmacyRepository pharmacyRepository,
    IMedicationOrderRepository medicationOrderRepository,
    IDeliveryAgentAssignmentService deliveryAgentAssignmentService,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    TimeProvider timeProvider)
    : IRequestHandler<ConfirmMedicationOrderCommand, MedicationOrderDto>
{
    public async Task<MedicationOrderDto> Handle(ConfirmMedicationOrderCommand request, CancellationToken ct)
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

        var confirmedAtUtc = timeProvider.GetUtcNow().UtcDateTime;

        try
        {
            if (order.DeliveryType == MedicationDeliveryType.Delivery)
            {
                var assignment = await deliveryAgentAssignmentService.AssignForOrderAsync(order.Id, ct);
                order.ConfirmForDelivery(assignment.AgentName, assignment.TrackingUrl, confirmedAtUtc);
            }
            else
            {
                order.ConfirmForPickup(confirmedAtUtc);
            }
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
