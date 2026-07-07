namespace HealthPlatform.Application.Notifications;

public interface INotificationLogWriter
{
    Task RecordDispatchAsync(
        NotificationDispatchRequest request,
        ResolvedNotificationRecipient recipient,
        IReadOnlyList<ChannelDeliveryResult> channelResults,
        CancellationToken ct);
}
