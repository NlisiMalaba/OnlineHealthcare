using HealthPlatform.Domain.Notifications;

namespace HealthPlatform.Application.Notifications;

public sealed class NotificationLogWriter(
    TimeProvider timeProvider,
    INotificationLogRepository repository) : INotificationLogWriter
{
    public async Task RecordDispatchAsync(
        NotificationDispatchRequest request,
        ResolvedNotificationRecipient recipient,
        IReadOnlyList<ChannelDeliveryResult> channelResults,
        CancellationToken ct)
    {
        if (channelResults.Count == 0)
        {
            return;
        }

        ct.ThrowIfCancellationRequested();

        var recipientId = NotificationLogRecipientIdResolver.Resolve(request);
        var sentAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var payloadJson = NotificationLogMappings.SerializePayload(request.Metadata);
        var recipientType = NotificationLogMappings.ToRecipientTypeKey(request.RecipientType);

        var entries = channelResults
            .Select(result => NotificationLog.RecordAttempt(
                recipientId,
                recipientType,
                NotificationLogMappings.ToChannelKey(result.Channel),
                request.EventType,
                payloadJson,
                NotificationLogMappings.ToDeliveryStatus(result.Succeeded),
                attempts: 1,
                sentAtUtc,
                deliveredAtUtc: null,
                result.FailureReason))
            .ToList();

        await repository.AddRangeAsync(entries, ct);
        await repository.SaveChangesAsync(ct);

        _ = recipient;
    }
}
