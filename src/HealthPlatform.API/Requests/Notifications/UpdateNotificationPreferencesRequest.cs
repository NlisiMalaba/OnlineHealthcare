namespace HealthPlatform.API.Requests.Notifications;

public sealed class UpdateNotificationPreferencesRequest
{
    public required IReadOnlyList<NotificationEventPreferenceUpdateRequest> Preferences { get; init; }
}

public sealed class NotificationEventPreferenceUpdateRequest
{
    public required string EventType { get; init; }

    public required IReadOnlyList<NotificationChannelPreferenceUpdateRequest> Channels { get; init; }
}

public sealed class NotificationChannelPreferenceUpdateRequest
{
    public required string Channel { get; init; }

    public required bool IsEnabled { get; init; }
}
