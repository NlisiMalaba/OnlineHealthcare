using HealthPlatform.Domain.Pharmacy;

namespace HealthPlatform.Application.PharmacyOrders.Inventory;

internal static class InventoryItemMappings
{
    public static InventoryItemDto ToDto(this InventoryItem item) =>
        new(
            item.Id,
            item.PharmacyId,
            item.MedicationName,
            item.MedicationSku,
            item.Quantity,
            item.LowStockThreshold,
            item.Quantity == 0,
            item.UpdatedAtUtc);
}
