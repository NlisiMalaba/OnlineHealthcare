using HealthPlatform.Application.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Notifications.Gateways;

public sealed class AfricasTalkingSmsNotificationGateway(
    IOptions<NotificationChannelsOptions> options,
    ILogger<AfricasTalkingSmsNotificationGateway> logger) : ISmsNotificationGateway
{
    private readonly AfricasTalkingSmsOptions _options = options.Value.Sms.AfricasTalking;

    public string Provider => NotificationChannelProviders.AfricasTalking;

    public bool IsConfigured =>
        _options.Enabled
        && !string.IsNullOrWhiteSpace(_options.ApiKey)
        && !string.IsNullOrWhiteSpace(_options.Username);

    public Task<bool> TrySendAsync(SmsNotificationDeliveryRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.Recipient.PhoneNumberE164))
        {
            return Task.FromResult(false);
        }

        logger.LogInformation(
            "Africa's Talking SMS dispatch requested for event {EventType}, recipient user {UserId}.",
            request.EventType,
            request.Recipient.UserId);
        return Task.FromResult(true);
    }
}
