namespace HealthPlatform.Application.Notifications;

public sealed record NotificationDispatchRequest(
    Guid? UserId,
    NotificationRecipientType RecipientType,
    string EventType,
    NotificationCriticality Criticality,
    NotificationContent Content,
    NotificationContactOverride? ContactOverride = null,
    IReadOnlyDictionary<string, string>? Metadata = null,
    IReadOnlyList<NotificationChannel>? Channels = null);
