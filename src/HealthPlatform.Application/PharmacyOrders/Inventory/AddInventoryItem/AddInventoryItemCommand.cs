using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.PharmacyOrders.Inventory.AddInventoryItem;

public sealed record AddInventoryItemCommand(
    string MedicationName,
    string MedicationSku,
    int Quantity,
    int? LowStockThreshold) : ICommand<InventoryItemDto>;
