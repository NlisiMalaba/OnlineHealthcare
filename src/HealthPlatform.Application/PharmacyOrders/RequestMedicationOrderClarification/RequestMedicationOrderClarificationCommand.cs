using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.PharmacyOrders.RequestMedicationOrderClarification;

public sealed record RequestMedicationOrderClarificationCommand(
    Guid OrderId,
    string Message) : ICommand<MedicationOrderDto>;
