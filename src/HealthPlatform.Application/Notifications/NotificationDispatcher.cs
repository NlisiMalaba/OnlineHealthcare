using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Notifications;

public sealed class NotificationDispatcher(
    INotificationPreferenceResolver preferenceResolver,
    INotificationRecipientResolver recipientResolver,
    INotificationChannelGatewayResolver gatewayResolver,
    INotificationLogWriter notificationLogWriter,
    ILogger<NotificationDispatcher> logger) : INotificationDispatcher
{
    public async Task<NotificationDispatchResult> DispatchAsync(
        NotificationDispatchRequest request,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var recipient = await ResolveRecipientAsync(request, ct);
        var enabledChannels = request.Channels
            ?? await preferenceResolver.ResolveEnabledChannelsAsync(
                request.UserId,
                request.EventType,
                request.Criticality,
                ct);

        var channelResults = new List<ChannelDeliveryResult>();
        foreach (var channel in enabledChannels)
        {
            var result = await DeliverOnChannelAsync(request, recipient, channel, ct);
            channelResults.Add(result);
        }

        if (request.Criticality == NotificationCriticality.Critical
            && channelResults.Any(result => result.Channel == NotificationChannel.Push && !result.Succeeded)
            && enabledChannels.All(channel => channel != NotificationChannel.Sms)
            && channelResults.All(result => result.Channel != NotificationChannel.Sms))
        {
            var smsFallback = await DeliverOnChannelAsync(
                request,
                recipient,
                NotificationChannel.Sms,
                ct);
            channelResults.Add(smsFallback);
        }

        logger.LogInformation(
            "Notification dispatch completed for event {EventType}, recipient type {RecipientType}, channels attempted {ChannelCount}, any succeeded {AnySucceeded}.",
            request.EventType,
            request.RecipientType,
            channelResults.Count,
            channelResults.Any(result => result.Succeeded));

        await notificationLogWriter.RecordDispatchAsync(request, recipient, channelResults, ct);

        return new NotificationDispatchResult(channelResults);
    }

    private async Task<ResolvedNotificationRecipient> ResolveRecipientAsync(
        NotificationDispatchRequest request,
        CancellationToken ct)
    {
        if (request.ContactOverride is not null)
        {
            return new ResolvedNotificationRecipient(
                request.UserId,
                request.RecipientType,
                request.ContactOverride.Email,
                request.ContactOverride.PhoneNumberE164,
                request.ContactOverride.PushTokens ?? []);
        }

        if (!request.UserId.HasValue)
        {
            throw new InvalidOperationException(
                "A user id or contact override is required to resolve notification recipients.");
        }

        return await recipientResolver.ResolveAsync(request.UserId.Value, request.RecipientType, ct);
    }

    private async Task<ChannelDeliveryResult> DeliverOnChannelAsync(
        NotificationDispatchRequest request,
        ResolvedNotificationRecipient recipient,
        NotificationChannel channel,
        CancellationToken ct)
    {
        try
        {
            var succeeded = channel switch
            {
                NotificationChannel.Push => await gatewayResolver.ResolvePush().TrySendAsync(
                    new PushNotificationDeliveryRequest(recipient, request.EventType, request.Content),
                    ct),
                NotificationChannel.Sms => await gatewayResolver.ResolveSms().TrySendAsync(
                    new SmsNotificationDeliveryRequest(recipient, request.EventType, request.Content),
                    ct),
                NotificationChannel.Email => await gatewayResolver.ResolveEmail().TrySendAsync(
                    new EmailNotificationDeliveryRequest(recipient, request.EventType, request.Content),
                    ct),
                _ => false
            };

            return new ChannelDeliveryResult(channel, succeeded, succeeded ? null : "DELIVERY_FAILED");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(
                ex,
                "Notification delivery failed on channel {Channel} for event {EventType}.",
                channel,
                request.EventType);
            return new ChannelDeliveryResult(channel, false, "DELIVERY_EXCEPTION");
        }
    }
}
