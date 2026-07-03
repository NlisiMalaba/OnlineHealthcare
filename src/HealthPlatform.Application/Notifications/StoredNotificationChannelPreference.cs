namespace HealthPlatform.Application.Notifications;

public sealed record StoredNotificationChannelPreference(
    string EventType,
    string Channel,
    bool IsEnabled);
