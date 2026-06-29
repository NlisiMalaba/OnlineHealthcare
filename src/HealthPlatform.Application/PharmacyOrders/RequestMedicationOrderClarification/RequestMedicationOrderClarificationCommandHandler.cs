using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Pharmacy;
using MediatR;

namespace HealthPlatform.Application.PharmacyOrders.RequestMedicationOrderClarification;

public sealed class RequestMedicationOrderClarificationCommandHandler(
    ICurrentUserAccessor currentUser,
    IPharmacyRepository pharmacyRepository,
    IMedicationOrderRepository medicationOrderRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    TimeProvider timeProvider)
    : IRequestHandler<RequestMedicationOrderClarificationCommand, MedicationOrderDto>
{
    public async Task<MedicationOrderDto> Handle(
        RequestMedicationOrderClarificationCommand request,
        CancellationToken ct)
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
            order.RequestClarification(request.Message, timeProvider.GetUtcNow().UtcDateTime);
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
