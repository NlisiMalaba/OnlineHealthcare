using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.PharmacyOrders;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingPharmacyOrderReceivedNotifier(INotificationDispatcher dispatcher)
    : IPharmacyOrderReceivedNotifier
{
    public Task NotifyOrderReceivedAsync(
        Guid pharmacyUserId,
        Guid orderId,
        Guid prescriptionId,
        string medicationName,
        string? deliveryAddress,
        CancellationToken ct) =>
        NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            pharmacyUserId,
            NotificationRecipientType.Pharmacy,
            NotificationEventTypes.OrderReceived,
            NotificationCriticality.Standard,
            "New medication order",
            "A new medication order has been received.",
            new Dictionary<string, string>
            {
                ["order_id"] = orderId.ToString(),
                ["prescription_id"] = prescriptionId.ToString()
            },
            ct);
}
