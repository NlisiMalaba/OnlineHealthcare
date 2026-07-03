using HealthPlatform.Application.Notifications;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Notifications.Gateways;

public sealed class LoggingEmailNotificationGateway(ILogger<LoggingEmailNotificationGateway> logger)
    : IEmailNotificationGateway
{
    public string Provider => NotificationChannelProviders.Logging;

    public bool IsConfigured => true;

    public Task<bool> TrySendAsync(EmailNotificationDeliveryRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.Recipient.Email))
        {
            logger.LogWarning(
                "Email notification skipped for event {EventType}; recipient has no email address.",
                request.EventType);
            return Task.FromResult(false);
        }

        logger.LogInformation(
            "Email notification dispatch requested for event {EventType}, recipient user {UserId}.",
            request.EventType,
            request.Recipient.UserId);
        return Task.FromResult(true);
    }
}
