using HealthPlatform.Application.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Notifications.Gateways;

public sealed class FcmPushNotificationGateway(
    IOptions<NotificationChannelsOptions> options,
    ILogger<FcmPushNotificationGateway> logger) : IPushNotificationGateway
{
    private readonly FcmPushOptions _options = options.Value.Push.Fcm;

    public string Provider => NotificationChannelProviders.Fcm;

    public bool IsConfigured =>
        _options.Enabled
        && !string.IsNullOrWhiteSpace(_options.ProjectId)
        && !string.IsNullOrWhiteSpace(_options.ServiceAccountJson);

    public Task<bool> TrySendAsync(PushNotificationDeliveryRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (request.Recipient.PushTokens.Count == 0)
        {
            return Task.FromResult(false);
        }

        logger.LogInformation(
            "FCM push dispatch requested for event {EventType}, recipient user {UserId}, token count {TokenCount}.",
            request.EventType,
            request.Recipient.UserId,
            request.Recipient.PushTokens.Count);

        return Task.FromResult(true);
    }
}
