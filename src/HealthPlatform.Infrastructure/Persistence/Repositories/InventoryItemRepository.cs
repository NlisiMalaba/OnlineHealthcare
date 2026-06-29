using HealthPlatform.Application.PharmacyOrders.Inventory;
using HealthPlatform.Domain.Pharmacy;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class InventoryItemRepository(ApplicationDbContext db) : IInventoryItemRepository
{
    public Task<bool> ExistsByPharmacyAndSkuAsync(Guid pharmacyId, string medicationSku, CancellationToken ct) =>
        db.InventoryItems.AnyAsync(
            item => item.PharmacyId == pharmacyId && item.MedicationSku == medicationSku.Trim(),
            ct);

    public Task<InventoryItem?> GetByIdForPharmacyAsync(Guid itemId, Guid pharmacyId, CancellationToken ct) =>
        db.InventoryItems.SingleOrDefaultAsync(
            item => item.Id == itemId && item.PharmacyId == pharmacyId,
            ct);

    public async Task<IReadOnlyList<InventoryItem>> ListByPharmacyIdAsync(Guid pharmacyId, CancellationToken ct) =>
        await db.InventoryItems
            .Where(item => item.PharmacyId == pharmacyId)
            .OrderBy(item => item.MedicationName)
            .ToListAsync(ct);

    public async Task AddAsync(InventoryItem item, CancellationToken ct)
    {
        await db.InventoryItems.AddAsync(item, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task UpdateAsync(InventoryItem item, CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
