using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.PharmacyOrders;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingLowStockAlertNotifier(INotificationDispatcher dispatcher)
    : ILowStockAlertNotifier
{
    public Task NotifyLowStockAsync(
        Guid pharmacyUserId,
        Guid inventoryItemId,
        string medicationSku,
        string medicationName,
        int quantity,
        int lowStockThreshold,
        CancellationToken ct) =>
        NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            pharmacyUserId,
            NotificationRecipientType.Pharmacy,
            NotificationEventTypes.LowStockAlert,
            NotificationCriticality.Standard,
            "Low stock alert",
            "A medication item is below the low stock threshold.",
            new Dictionary<string, string>
            {
                ["inventory_item_id"] = inventoryItemId.ToString(),
                ["medication_sku"] = medicationSku,
                ["quantity"] = quantity.ToString(),
                ["low_stock_threshold"] = lowStockThreshold.ToString()
            },
            ct);
}
