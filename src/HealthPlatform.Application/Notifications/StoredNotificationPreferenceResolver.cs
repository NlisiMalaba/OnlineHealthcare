namespace HealthPlatform.Application.Notifications;

public sealed class StoredNotificationPreferenceResolver(
    INotificationPreferenceService preferenceService) : INotificationPreferenceResolver
{
    public async Task<IReadOnlyList<NotificationChannel>> ResolveEnabledChannelsAsync(
        Guid? userId,
        string eventType,
        NotificationCriticality criticality,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _ = criticality;

        if (!userId.HasValue)
        {
            return NotificationPreferenceDefaults.AllChannels;
        }

        var stored = await preferenceService.GetStoredPreferencesAsync(userId.Value, ct);
        return NotificationPreferenceDefaults.ResolveEnabledChannels(eventType, stored);
    }
}
