using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Pharmacy;

public sealed class InventoryItem : Entity
{
    private InventoryItem()
    {
        MedicationName = string.Empty;
        MedicationSku = string.Empty;
    }

    public Guid PharmacyId { get; private set; }

    public string MedicationName { get; private set; }

    public string MedicationSku { get; private set; }

    public int Quantity { get; private set; }

    public int LowStockThreshold { get; private set; }

    public static InventoryItem Add(
        Guid pharmacyId,
        string medicationName,
        string medicationSku,
        int quantity,
        int? lowStockThreshold)
    {
        if (pharmacyId == Guid.Empty)
        {
            throw new ArgumentException("Pharmacy id is required.", nameof(pharmacyId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(medicationName);
        ArgumentException.ThrowIfNullOrWhiteSpace(medicationSku);

        if (quantity < 0)
        {
            throw new ArgumentException("Quantity cannot be negative.", nameof(quantity));
        }

        var threshold = lowStockThreshold ?? InventoryPolicies.DefaultLowStockThreshold;
        if (threshold < 0)
        {
            throw new ArgumentException("Low stock threshold cannot be negative.", nameof(lowStockThreshold));
        }

        return new InventoryItem
        {
            Id = Guid.CreateVersion7(),
            PharmacyId = pharmacyId,
            MedicationName = medicationName.Trim(),
            MedicationSku = medicationSku.Trim(),
            Quantity = quantity,
            LowStockThreshold = threshold
        };
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity < 0)
        {
            throw new ArgumentException("Quantity cannot be negative.", nameof(quantity));
        }

        Quantity = quantity;
        Touch();
    }

    public void MarkOutOfStock()
    {
        Quantity = 0;
        Touch();
    }
}
