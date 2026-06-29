namespace HealthPlatform.Application.PharmacyOrders.Inventory;

public sealed record InventoryItemDto(
    Guid Id,
    Guid PharmacyId,
    string MedicationName,
    string MedicationSku,
    int Quantity,
    int LowStockThreshold,
    bool IsOutOfStock,
    DateTime UpdatedAtUtc);
