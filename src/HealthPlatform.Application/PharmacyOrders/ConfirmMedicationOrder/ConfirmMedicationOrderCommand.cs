using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.PharmacyOrders.ConfirmMedicationOrder;

public sealed record ConfirmMedicationOrderCommand(Guid OrderId) : ICommand<MedicationOrderDto>;
