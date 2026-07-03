namespace HealthPlatform.Application.Notifications;

public interface INotificationRecipientResolver
{
    Task<ResolvedNotificationRecipient> ResolveAsync(
        Guid userId,
        NotificationRecipientType recipientType,
        CancellationToken ct);
}
