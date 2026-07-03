namespace HealthPlatform.Application.Notifications;

public static class NotificationPreferenceDefaults
{
    public static readonly IReadOnlyList<NotificationChannel> AllChannels =
    [
        NotificationChannel.Push,
        NotificationChannel.Email,
        NotificationChannel.Sms
    ];

    public static IReadOnlyList<NotificationChannel> ResolveEnabledChannels(
        string eventType,
        IReadOnlyList<StoredNotificationChannelPreference> storedPreferences)
    {
        var enabled = new List<NotificationChannel>(AllChannels.Count);
        foreach (var channel in AllChannels)
        {
            var channelKey = ToChannelKey(channel);
            var stored = storedPreferences.FirstOrDefault(
                preference => preference.EventType == eventType && preference.Channel == channelKey);

            if (stored is null || stored.IsEnabled)
            {
                enabled.Add(channel);
            }
        }

        return enabled;
    }

    public static NotificationChannelSettingsDto CreateChannelSettings(
        string eventType,
        IReadOnlyList<StoredNotificationChannelPreference> storedPreferences) =>
        new(
            Push: IsChannelEnabled(eventType, NotificationChannel.Push, storedPreferences),
            Email: IsChannelEnabled(eventType, NotificationChannel.Email, storedPreferences),
            Sms: IsChannelEnabled(eventType, NotificationChannel.Sms, storedPreferences));

    public static string ToChannelKey(NotificationChannel channel) =>
        channel switch
        {
            NotificationChannel.Push => "push",
            NotificationChannel.Sms => "sms",
            NotificationChannel.Email => "email",
            _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Unsupported notification channel.")
        };

    public static bool TryParseChannelKey(string channel, out NotificationChannel parsed) =>
        channel.ToLowerInvariant() switch
        {
            "push" => Assign(NotificationChannel.Push, out parsed),
            "sms" => Assign(NotificationChannel.Sms, out parsed),
            "email" => Assign(NotificationChannel.Email, out parsed),
            _ => Assign(default, out parsed) && false
        };

    private static bool Assign(NotificationChannel value, out NotificationChannel parsed)
    {
        parsed = value;
        return true;
    }

    private static bool IsChannelEnabled(
        string eventType,
        NotificationChannel channel,
        IReadOnlyList<StoredNotificationChannelPreference> storedPreferences)
    {
        var channelKey = ToChannelKey(channel);
        var stored = storedPreferences.FirstOrDefault(
            preference => preference.EventType == eventType && preference.Channel == channelKey);
        return stored is null || stored.IsEnabled;
    }
}
