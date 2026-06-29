using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.PharmacyOrders.MarkMedicationOrderDispatched;

public sealed record MarkMedicationOrderDispatchedCommand(Guid OrderId) : ICommand<MedicationOrderDto>;
