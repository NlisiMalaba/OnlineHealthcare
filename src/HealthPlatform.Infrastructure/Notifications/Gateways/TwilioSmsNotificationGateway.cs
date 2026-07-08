using HealthPlatform.Application.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Notifications.Gateways;

public sealed class TwilioSmsNotificationGateway(
    IOptions<NotificationChannelsOptions> options,
    ILogger<TwilioSmsNotificationGateway> logger) : ISmsNotificationGateway
{
    private readonly TwilioSmsOptions _options = options.Value.Sms.Twilio;

    public string Provider => NotificationChannelProviders.Twilio;

    public bool IsConfigured =>
        _options.Enabled
        && !string.IsNullOrWhiteSpace(_options.AccountSid)
        && !string.IsNullOrWhiteSpace(_options.AuthToken)
        && !string.IsNullOrWhiteSpace(_options.FromNumber);

    public Task<bool> TrySendAsync(SmsNotificationDeliveryRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.Recipient.PhoneNumberE164))
        {
            return Task.FromResult(false);
        }

        logger.LogInformation(
            "Twilio SMS dispatch requested for event {EventType}, recipient user {UserId}.",
            request.EventType,
            request.Recipient.UserId);
        return Task.FromResult(true);
    }
}
