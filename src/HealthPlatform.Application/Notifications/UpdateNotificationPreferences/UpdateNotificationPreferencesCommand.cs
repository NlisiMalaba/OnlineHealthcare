using HealthPlatform.Application.Notifications.UpdateNotificationPreferences;
using MediatR;

namespace HealthPlatform.Application.Notifications.UpdateNotificationPreferences;

public sealed record UpdateNotificationPreferencesCommand(
    IReadOnlyList<NotificationEventPreferenceUpdateDto> Preferences)
    : IRequest<NotificationPreferencesDto>;
