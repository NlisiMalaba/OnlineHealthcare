namespace HealthPlatform.Application.Notifications;

public sealed record PushNotificationDeliveryRequest(
    ResolvedNotificationRecipient Recipient,
    string EventType,
    NotificationContent Content);

public interface IPushNotificationGateway
{
    string Provider { get; }

    bool IsConfigured { get; }

    Task<bool> TrySendAsync(PushNotificationDeliveryRequest request, CancellationToken ct);
}
