using HealthPlatform.Application.PharmacyOrders;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingLowStockAlertNotifier : ILowStockAlertNotifier
{
    public List<(
        Guid PharmacyUserId,
        Guid InventoryItemId,
        string MedicationSku,
        string MedicationName,
        int Quantity,
        int LowStockThreshold)> Notifications { get; } = [];

    public Task NotifyLowStockAsync(
        Guid pharmacyUserId,
        Guid inventoryItemId,
        string medicationSku,
        string medicationName,
        int quantity,
        int lowStockThreshold,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Notifications.Add((
            pharmacyUserId,
            inventoryItemId,
            medicationSku,
            medicationName,
            quantity,
            lowStockThreshold));
        return Task.CompletedTask;
    }
}
