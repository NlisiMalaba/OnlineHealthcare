namespace HealthPlatform.Application.Notifications;

public sealed record NotificationDispatchResult(
    IReadOnlyList<ChannelDeliveryResult> ChannelResults)
{
    public bool AnySucceeded => ChannelResults.Any(result => result.Succeeded);

    public bool PushFailed =>
        ChannelResults.Any(result => result.Channel == NotificationChannel.Push && !result.Succeeded);
}
