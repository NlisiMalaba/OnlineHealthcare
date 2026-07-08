namespace HealthPlatform.Application.Notifications;

public sealed record NotificationContactOverride(
    string? Email,
    string? PhoneNumberE164,
    IReadOnlyList<string>? PushTokens = null);
