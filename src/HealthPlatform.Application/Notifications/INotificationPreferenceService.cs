namespace HealthPlatform.Application.Notifications;

public interface INotificationPreferenceService
{
    Task<IReadOnlyList<StoredNotificationChannelPreference>> GetStoredPreferencesAsync(
        Guid userId,
        CancellationToken ct);

    Task<NotificationPreferencesDto> GetPreferencesForRolesAsync(
        Guid userId,
        IReadOnlyList<string> roles,
        CancellationToken ct);
}
