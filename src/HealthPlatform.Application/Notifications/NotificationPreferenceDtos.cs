namespace HealthPlatform.Application.Notifications;

public sealed record NotificationChannelSettingsDto(
    bool Push,
    bool Email,
    bool Sms);

public sealed record NotificationEventPreferenceDto(
    string EventType,
    NotificationChannelSettingsDto Channels);

public sealed record NotificationPreferencesDto(
    IReadOnlyList<NotificationEventPreferenceDto> Preferences);

public sealed record NotificationChannelPreferenceUpdateDto(
    string Channel,
    bool IsEnabled);

public sealed record NotificationEventPreferenceUpdateDto(
    string EventType,
    IReadOnlyList<NotificationChannelPreferenceUpdateDto> Channels);
