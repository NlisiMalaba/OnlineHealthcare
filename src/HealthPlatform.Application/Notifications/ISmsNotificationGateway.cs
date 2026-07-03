namespace HealthPlatform.Application.Notifications;

public sealed record SmsNotificationDeliveryRequest(
    ResolvedNotificationRecipient Recipient,
    string EventType,
    NotificationContent Content);

public interface ISmsNotificationGateway
{
    string Provider { get; }

    bool IsConfigured { get; }

    Task<bool> TrySendAsync(SmsNotificationDeliveryRequest request, CancellationToken ct);
}
