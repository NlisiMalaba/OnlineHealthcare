using HealthPlatform.Application.Notifications;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Notifications.Gateways;

public sealed class LoggingPushNotificationGateway(ILogger<LoggingPushNotificationGateway> logger)
    : IPushNotificationGateway
{
    public string Provider => NotificationChannelProviders.Logging;

    public bool IsConfigured => true;

    public Task<bool> TrySendAsync(PushNotificationDeliveryRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Push notification dispatch requested for event {EventType}, recipient user {UserId}, token count {TokenCount}.",
            request.EventType,
            request.Recipient.UserId,
            request.Recipient.PushTokens.Count);
        return Task.FromResult(request.Recipient.PushTokens.Count > 0);
    }
}
