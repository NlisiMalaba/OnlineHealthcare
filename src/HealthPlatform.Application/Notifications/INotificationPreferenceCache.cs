namespace HealthPlatform.Application.Notifications;

public interface INotificationPreferenceCache
{
    Task<IReadOnlyList<StoredNotificationChannelPreference>?> GetAsync(Guid userId, CancellationToken ct);

    Task SetAsync(
        Guid userId,
        IReadOnlyList<StoredNotificationChannelPreference> preferences,
        CancellationToken ct);

    Task InvalidateAsync(Guid userId, CancellationToken ct);
}
