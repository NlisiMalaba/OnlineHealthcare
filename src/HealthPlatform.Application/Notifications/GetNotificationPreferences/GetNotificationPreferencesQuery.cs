using MediatR;

namespace HealthPlatform.Application.Notifications.GetNotificationPreferences;

public sealed record GetNotificationPreferencesQuery : IRequest<NotificationPreferencesDto>;
