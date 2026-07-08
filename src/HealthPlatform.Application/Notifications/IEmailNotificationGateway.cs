namespace HealthPlatform.Application.Notifications;

public sealed record EmailNotificationDeliveryRequest(
    ResolvedNotificationRecipient Recipient,
    string EventType,
    NotificationContent Content);

public interface IEmailNotificationGateway
{
    string Provider { get; }

    bool IsConfigured { get; }

    Task<bool> TrySendAsync(EmailNotificationDeliveryRequest request, CancellationToken ct);
}
