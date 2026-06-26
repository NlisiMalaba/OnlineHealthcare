using HealthPlatform.API.Requests.Pharmacy;
using HealthPlatform.Application.PharmacyOrders.Inventory.AddInventoryItem;
using HealthPlatform.Application.PharmacyOrders.Inventory.MarkInventoryItemOutOfStock;
using HealthPlatform.Application.PharmacyOrders.Inventory.UpdateInventoryItemQuantity;

namespace HealthPlatform.API.Mapping;

public static class PharmacyInventoryCommandMapper
{
    public static AddInventoryItemCommand ToAddCommand(AddInventoryItemRequest request) =>
        new(
            request.MedicationName,
            request.MedicationSku,
            request.Quantity,
            request.LowStockThreshold);

    public static UpdateInventoryItemQuantityCommand ToUpdateQuantityCommand(
        Guid inventoryItemId,
        UpdateInventoryItemQuantityRequest request) =>
        new(inventoryItemId, request.Quantity);

    public static MarkInventoryItemOutOfStockCommand ToMarkOutOfStockCommand(Guid inventoryItemId) =>
        new(inventoryItemId);
}
