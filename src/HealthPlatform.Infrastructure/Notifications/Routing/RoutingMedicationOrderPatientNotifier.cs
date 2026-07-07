using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.PharmacyOrders;
using System.Text.Json;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingMedicationOrderPatientNotifier(INotificationDispatcher dispatcher)
    : IMedicationOrderPatientNotifier
{
    public Task NotifyOrderStatusChangedAsync(
        Guid patientUserId,
        Guid orderId,
        string previousStatus,
        string newStatus,
        string? trackingUrl,
        IReadOnlyList<PharmacyOrderAlternativeDto>? alternativePharmacies,
        CancellationToken ct)
    {
        var data = new Dictionary<string, string>
        {
            ["order_id"] = orderId.ToString(),
            ["previous_status"] = previousStatus,
            ["new_status"] = newStatus
        };

        if (!string.IsNullOrWhiteSpace(trackingUrl))
        {
            data["tracking_url"] = trackingUrl;
        }

        if (alternativePharmacies is { Count: > 0 })
        {
            data["alternative_pharmacies"] = JsonSerializer.Serialize(
                alternativePharmacies.Select(pharmacy => new
                {
                    pharmacy.PharmacyId,
                    pharmacy.Name,
                    pharmacy.Address
                }));
        }

        return NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            patientUserId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.OrderStatusChanged,
            NotificationCriticality.Standard,
            "Order status updated",
            $"Your medication order status is now {newStatus}.",
            data,
            ct);
    }
}
