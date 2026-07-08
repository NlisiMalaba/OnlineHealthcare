using HealthPlatform.Application.Notifications;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Notifications.Gateways;

public sealed class LoggingSmsNotificationGateway(ILogger<LoggingSmsNotificationGateway> logger)
    : ISmsNotificationGateway
{
    public string Provider => NotificationChannelProviders.Logging;

    public bool IsConfigured => true;

    public Task<bool> TrySendAsync(SmsNotificationDeliveryRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.Recipient.PhoneNumberE164))
        {
            logger.LogWarning(
                "SMS notification skipped for event {EventType}; recipient has no phone number.",
                request.EventType);
            return Task.FromResult(false);
        }

        logger.LogInformation(
            "SMS notification dispatch requested for event {EventType}, recipient user {UserId}.",
            request.EventType,
            request.Recipient.UserId);
        return Task.FromResult(true);
    }
}
