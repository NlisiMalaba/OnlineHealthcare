using HealthPlatform.Application.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Notifications.Gateways;

public sealed class SendGridEmailNotificationGateway(
    IOptions<NotificationChannelsOptions> options,
    ILogger<SendGridEmailNotificationGateway> logger) : IEmailNotificationGateway
{
    private readonly SendGridEmailOptions _options = options.Value.Email.SendGrid;

    public string Provider => NotificationChannelProviders.SendGrid;

    public bool IsConfigured =>
        _options.Enabled
        && !string.IsNullOrWhiteSpace(_options.ApiKey)
        && !string.IsNullOrWhiteSpace(_options.FromEmail);

    public Task<bool> TrySendAsync(EmailNotificationDeliveryRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.Recipient.Email))
        {
            return Task.FromResult(false);
        }

        logger.LogInformation(
            "SendGrid email dispatch requested for event {EventType}, recipient user {UserId}.",
            request.EventType,
            request.Recipient.UserId);
        return Task.FromResult(true);
    }
}
