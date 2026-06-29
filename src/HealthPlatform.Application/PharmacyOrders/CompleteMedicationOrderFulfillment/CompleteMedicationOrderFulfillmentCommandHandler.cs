using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Pharmacy;
using MediatR;

namespace HealthPlatform.Application.PharmacyOrders.CompleteMedicationOrderFulfillment;

public sealed class CompleteMedicationOrderFulfillmentCommandHandler(
    ICurrentUserAccessor currentUser,
    IPharmacyRepository pharmacyRepository,
    IMedicationOrderRepository medicationOrderRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    TimeProvider timeProvider)
    : IRequestHandler<CompleteMedicationOrderFulfillmentCommand, MedicationOrderDto>
{
    public async Task<MedicationOrderDto> Handle(CompleteMedicationOrderFulfillmentCommand request, CancellationToken ct)
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

        var completedAtUtc = timeProvider.GetUtcNow().UtcDateTime;

        try
        {
            if (order.DeliveryType == MedicationDeliveryType.Delivery)
            {
                order.MarkDelivered(completedAtUtc);
            }
            else
            {
                order.MarkPickedUp(completedAtUtc);
            }
        }
        catch (MedicationOrderNotActionableException exception)
        {
            throw MedicationOrderWorkflowSupport.MapNotActionable(exception);
        }
        catch (InvalidOperationException exception)
        {
            throw new DomainException(PharmacyErrorCodes.OrderNotActionable, exception.Message);
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
