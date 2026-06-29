using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Pharmacy.Events;

public sealed record InventoryLowStockDetectedDomainEvent(
    Guid InventoryItemId,
    Guid PharmacyId,
    string MedicationSku,
    string MedicationName,
    int Quantity,
    int LowStockThreshold) : DomainEvent;
