using HealthPlatform.Application.Behaviors;
using HealthPlatform.Domain.Pharmacy;

namespace HealthPlatform.Application.PharmacyOrders.CreateMedicationOrder;

public sealed record CreateMedicationOrderCommand(
    Guid PrescriptionId,
    Guid PharmacyId,
    string MedicationSku,
    MedicationDeliveryType DeliveryType,
    string? DeliveryAddress) : ICommand<MedicationOrderDto>;
