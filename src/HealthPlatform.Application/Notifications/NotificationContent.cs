namespace HealthPlatform.Application.Notifications;

public sealed record NotificationContent(
    string Title,
    string Body,
    IReadOnlyDictionary<string, string>? Data = null);
