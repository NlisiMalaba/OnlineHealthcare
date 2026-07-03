namespace HealthPlatform.Application.Notifications;

public sealed record ChannelDeliveryResult(
    NotificationChannel Channel,
    bool Succeeded,
    string? FailureReason = null);
