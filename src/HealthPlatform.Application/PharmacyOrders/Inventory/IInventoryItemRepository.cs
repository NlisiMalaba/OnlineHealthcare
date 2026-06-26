using HealthPlatform.Domain.Pharmacy;

namespace HealthPlatform.Application.PharmacyOrders.Inventory;

public interface IInventoryItemRepository
{
    Task<bool> ExistsByPharmacyAndSkuAsync(Guid pharmacyId, string medicationSku, CancellationToken ct);

    Task<InventoryItem?> GetByIdForPharmacyAsync(Guid itemId, Guid pharmacyId, CancellationToken ct);

    Task<IReadOnlyList<InventoryItem>> ListByPharmacyIdAsync(Guid pharmacyId, CancellationToken ct);

    Task AddAsync(InventoryItem item, CancellationToken ct);

    Task UpdateAsync(InventoryItem item, CancellationToken ct);
}
