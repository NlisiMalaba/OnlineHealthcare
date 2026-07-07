namespace HealthPlatform.Application.Notifications;

public sealed class NotificationPreferenceService(
    INotificationPreferenceRepository repository,
    INotificationPreferenceCache cache) : INotificationPreferenceService
{
    public async Task<IReadOnlyList<StoredNotificationChannelPreference>> GetStoredPreferencesAsync(
        Guid userId,
        CancellationToken ct)
    {
        var cached = await cache.GetAsync(userId, ct);
        if (cached is not null)
        {
            return cached;
        }

        var stored = await repository.ListByUserIdAsync(userId, ct);
        var mapped = Map(stored);
        await cache.SetAsync(userId, mapped, ct);
        return mapped;
    }

    public async Task<NotificationPreferencesDto> GetPreferencesForRolesAsync(
        Guid userId,
        IReadOnlyList<string> roles,
        CancellationToken ct)
    {
        var eventTypes = NotificationPreferenceCatalog.GetConfigurableEventTypes(roles);
        var stored = await GetStoredPreferencesAsync(userId, ct);
        var preferences = eventTypes
            .Select(eventType => new NotificationEventPreferenceDto(
                eventType,
                NotificationPreferenceDefaults.CreateChannelSettings(eventType, stored)))
            .ToList();

        return new NotificationPreferencesDto(preferences);
    }

    private static IReadOnlyList<StoredNotificationChannelPreference> Map(
        IReadOnlyList<Domain.Notifications.UserNotificationPreference> stored) =>
        stored
            .Select(preference => new StoredNotificationChannelPreference(
                preference.EventType,
                preference.Channel,
                preference.IsEnabled))
            .ToList();
}
