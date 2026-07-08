using HealthPlatform.API.Requests.Notifications;
using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.Notifications.UpdateNotificationPreferences;

namespace HealthPlatform.API.Mapping;

public static class NotificationPreferenceCommandMapper
{
    public static UpdateNotificationPreferencesCommand ToCommand(UpdateNotificationPreferencesRequest request) =>
        new(request.Preferences
            .Select(preference => new NotificationEventPreferenceUpdateDto(
                preference.EventType,
                preference.Channels
                    .Select(channel => new NotificationChannelPreferenceUpdateDto(
                        channel.Channel,
                        channel.IsEnabled))
                    .ToList()))
            .ToList());
}
