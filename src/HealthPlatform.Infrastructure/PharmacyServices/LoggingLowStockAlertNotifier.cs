using HealthPlatform.Application.PharmacyOrders;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.PharmacyServices;

public sealed class LoggingLowStockAlertNotifier(ILogger<LoggingLowStockAlertNotifier> logger)
    : ILowStockAlertNotifier
{
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

        logger.LogInformation(
            "Low-stock alert for pharmacy user {PharmacyUserId}. Inventory item {InventoryItemId}, medication {MedicationName} ({MedicationSku}), quantity {Quantity}, threshold {LowStockThreshold}.",
            pharmacyUserId,
            inventoryItemId,
            medicationName,
            medicationSku,
            quantity,
            lowStockThreshold);

        return Task.CompletedTask;
    }
}
