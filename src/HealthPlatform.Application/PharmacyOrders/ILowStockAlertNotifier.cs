namespace HealthPlatform.Application.PharmacyOrders;

public interface ILowStockAlertNotifier
{
    Task NotifyLowStockAsync(
        Guid pharmacyUserId,
        Guid inventoryItemId,
        string medicationSku,
        string medicationName,
        int quantity,
        int lowStockThreshold,
        CancellationToken ct);
}
