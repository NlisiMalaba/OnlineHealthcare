using HealthPlatform.Domain.Notifications;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Notifications;

public sealed class CriticalNotificationSmsFallbackService(
    TimeProvider timeProvider,
    ICriticalNotificationSmsFallbackRepository repository,
    ICriticalNotificationSmsFallbackScheduler scheduler,
    INotificationChannelGatewayResolver gatewayResolver,
    INotificationLogWriter notificationLogWriter,
    ILogger<CriticalNotificationSmsFallbackService> logger) : ICriticalNotificationSmsFallbackService
{
    public async Task ScheduleAsync(
        NotificationDispatchRequest request,
        ResolvedNotificationRecipient recipient,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!NotificationPolicies.RequiresSmsFallbackOnPushFailure(request.EventType))
        {
            return;
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var fallback = CriticalNotificationSmsFallback.CreatePending(
            NotificationLogRecipientIdResolver.Resolve(request),
            request.UserId,
            NotificationLogMappings.ToRecipientTypeKey(request.RecipientType),
            request.EventType,
            request.Content.Title,
            request.Content.Body,
            NotificationLogMappings.SerializePayload(request.Metadata),
            recipient.Email,
            recipient.PhoneNumberE164,
            nowUtc);

        await repository.AddAsync(fallback, ct);
        await repository.SaveChangesAsync(ct);
        scheduler.EnqueueImmediateProcessing(fallback.Id);

        logger.LogInformation(
            "Scheduled critical SMS fallback {FallbackId} for event {EventType}, recipient {RecipientId}.",
            fallback.Id,
            request.EventType,
            fallback.RecipientId);
    }

    public async Task<bool> ProcessAsync(Guid fallbackId, CancellationToken ct)
    {
        var fallback = await repository.GetByIdAsync(fallbackId, ct);
        if (fallback is null)
        {
            logger.LogWarning("Critical SMS fallback {FallbackId} was not found.", fallbackId);
            return false;
        }

        if (fallback.Status is CriticalNotificationSmsFallbackStatus.Sent
            or CriticalNotificationSmsFallbackStatus.FailedFinal)
        {
            return fallback.Status == CriticalNotificationSmsFallbackStatus.Sent;
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        if (!fallback.IsDue(nowUtc))
        {
            return false;
        }

        var succeeded = await AttemptSmsDeliveryAsync(fallback, ct);
        if (succeeded)
        {
            fallback.RecordSuccessfulAttempt(nowUtc);
        }
        else
        {
            fallback.RecordFailedAttempt(
                nowUtc,
                NotificationPolicies.MaxSmsFallbackRetries,
                NotificationPolicies.SmsFallbackRetryInterval);
        }

        await repository.UpdateAsync(fallback, ct);
        await repository.SaveChangesAsync(ct);

        if (fallback.Status == CriticalNotificationSmsFallbackStatus.FailedFinal)
        {
            logger.LogWarning(
                "Critical SMS fallback {FallbackId} for event {EventType} failed after {RetryCount} attempt(s).",
                fallback.Id,
                fallback.EventType,
                fallback.RetryCount);
        }

        return succeeded;
    }

    public async Task<int> ProcessDueAsync(CancellationToken ct)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var dueFallbacks = await repository.ListDueAsync(
            nowUtc,
            NotificationPolicies.SmsFallbackBatchSize,
            ct);

        var processed = 0;
        foreach (var fallback in dueFallbacks)
        {
            ct.ThrowIfCancellationRequested();
            await ProcessAsync(fallback.Id, ct);
            processed++;
        }

        return processed;
    }

    private async Task<bool> AttemptSmsDeliveryAsync(
        CriticalNotificationSmsFallback fallback,
        CancellationToken ct)
    {
        var recipient = new ResolvedNotificationRecipient(
            fallback.UserId,
            ParseRecipientType(fallback.RecipientType),
            fallback.Email,
            fallback.PhoneNumberE164,
            []);

        var request = new NotificationDispatchRequest(
            fallback.UserId,
            ParseRecipientType(fallback.RecipientType),
            fallback.EventType,
            NotificationCriticality.Critical,
            new NotificationContent(fallback.Title, fallback.Body),
            Metadata: DeserializeMetadata(fallback.PayloadJson),
            Channels: [NotificationChannel.Sms]);

        var succeeded = await gatewayResolver.ResolveSms().TrySendAsync(
            new SmsNotificationDeliveryRequest(recipient, fallback.EventType, request.Content),
            ct);

        await notificationLogWriter.RecordDispatchAsync(
            request,
            recipient,
            [new ChannelDeliveryResult(NotificationChannel.Sms, succeeded, succeeded ? null : "DELIVERY_FAILED")],
            ct);

        return succeeded;
    }

    private static NotificationRecipientType ParseRecipientType(string recipientType) =>
        recipientType switch
        {
            "patient" => NotificationRecipientType.Patient,
            "doctor" => NotificationRecipientType.Doctor,
            "pharmacy" => NotificationRecipientType.Pharmacy,
            "admin" => NotificationRecipientType.Admin,
            "next_of_kin" => NotificationRecipientType.NextOfKin,
            _ => NotificationRecipientType.Patient
        };

    private static IReadOnlyDictionary<string, string>? DeserializeMetadata(string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson) || payloadJson == "{}")
        {
            return null;
        }

        return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(payloadJson);
    }
}
