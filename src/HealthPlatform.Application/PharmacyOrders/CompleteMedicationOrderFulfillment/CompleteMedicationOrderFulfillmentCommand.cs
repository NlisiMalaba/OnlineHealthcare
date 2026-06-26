using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.PharmacyOrders.CompleteMedicationOrderFulfillment;

public sealed record CompleteMedicationOrderFulfillmentCommand(Guid OrderId) : ICommand<MedicationOrderDto>;
