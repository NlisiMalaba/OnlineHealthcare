namespace HealthPlatform.Application.Notifications;

public sealed class DefaultNotificationPreferenceResolver : INotificationPreferenceResolver
{
    private static readonly IReadOnlyList<NotificationChannel> DefaultChannels =
    [
        NotificationChannel.Push,
        NotificationChannel.Email,
        NotificationChannel.Sms
    ];

    public Task<IReadOnlyList<NotificationChannel>> ResolveEnabledChannelsAsync(
        Guid? userId,
        string eventType,
        NotificationCriticality criticality,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _ = userId;
        _ = eventType;

        return Task.FromResult<IReadOnlyList<NotificationChannel>>(DefaultChannels);
    }
}
