namespace HealthPlatform.Application.Notifications;

public sealed record ResolvedNotificationRecipient(
    Guid? UserId,
    NotificationRecipientType RecipientType,
    string? Email,
    string? PhoneNumberE164,
    IReadOnlyList<string> PushTokens);
