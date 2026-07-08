namespace HealthPlatform.Application.Notifications;

public interface INotificationPreferenceResolver
{
    Task<IReadOnlyList<NotificationChannel>> ResolveEnabledChannelsAsync(
        Guid? userId,
        string eventType,
        NotificationCriticality criticality,
        CancellationToken ct);
}
