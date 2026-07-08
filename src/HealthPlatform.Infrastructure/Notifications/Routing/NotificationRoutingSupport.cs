using HealthPlatform.Application.Notifications;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

internal static class NotificationRoutingSupport
{
    public static Task<NotificationDispatchResult> DispatchToUserAsync(
        INotificationDispatcher dispatcher,
        Guid userId,
        NotificationRecipientType recipientType,
        string eventType,
        NotificationCriticality criticality,
        string title,
        string body,
        IReadOnlyDictionary<string, string>? data,
        CancellationToken ct,
        IReadOnlyList<NotificationChannel>? channels = null) =>
        dispatcher.DispatchAsync(
            new NotificationDispatchRequest(
                userId,
                recipientType,
                eventType,
                criticality,
                new NotificationContent(title, body, data),
                Metadata: data,
                Channels: channels),
            ct);

    public static Task<NotificationDispatchResult> DispatchToContactAsync(
        INotificationDispatcher dispatcher,
        NotificationRecipientType recipientType,
        string eventType,
        NotificationCriticality criticality,
        string title,
        string body,
        NotificationContactOverride contactOverride,
        IReadOnlyDictionary<string, string>? data,
        CancellationToken ct,
        IReadOnlyList<NotificationChannel>? channels = null) =>
        dispatcher.DispatchAsync(
            new NotificationDispatchRequest(
                null,
                recipientType,
                eventType,
                criticality,
                new NotificationContent(title, body, data),
                contactOverride,
                data,
                channels),
            ct);
}
