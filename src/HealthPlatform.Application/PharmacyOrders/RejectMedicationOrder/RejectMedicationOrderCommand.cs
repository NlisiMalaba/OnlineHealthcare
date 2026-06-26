using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.PharmacyOrders.RejectMedicationOrder;

public sealed record RejectMedicationOrderCommand(Guid OrderId, string Reason) : ICommand<MedicationOrderDto>;
