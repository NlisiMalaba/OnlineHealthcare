using HealthPlatform.Application.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Notifications.Gateways;

public sealed class SesEmailNotificationGateway(
    IOptions<NotificationChannelsOptions> options,
    ILogger<SesEmailNotificationGateway> logger) : IEmailNotificationGateway
{
    private readonly SesEmailOptions _options = options.Value.Email.Ses;

    public string Provider => NotificationChannelProviders.Ses;

    public bool IsConfigured =>
        _options.Enabled
        && !string.IsNullOrWhiteSpace(_options.AccessKeyId)
        && !string.IsNullOrWhiteSpace(_options.SecretAccessKey)
        && !string.IsNullOrWhiteSpace(_options.Region)
        && !string.IsNullOrWhiteSpace(_options.FromEmail);

    public Task<bool> TrySendAsync(EmailNotificationDeliveryRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.Recipient.Email))
        {
            return Task.FromResult(false);
        }

        logger.LogInformation(
            "SES email dispatch requested for event {EventType}, recipient user {UserId}.",
            request.EventType,
            request.Recipient.UserId);
        return Task.FromResult(true);
    }
}
