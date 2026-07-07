using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Notifications;

public sealed class CriticalNotificationSmsFallback : Entity
{
    private CriticalNotificationSmsFallback()
    {
        RecipientType = string.Empty;
        EventType = string.Empty;
        Title = string.Empty;
        Body = string.Empty;
        PayloadJson = "{}";
    }

    public Guid RecipientId { get; private set; }

    public Guid? UserId { get; private set; }

    public string RecipientType { get; private set; }

    public string EventType { get; private set; }

    public string Title { get; private set; }

    public string Body { get; private set; }

    public string PayloadJson { get; private set; }

    public string? Email { get; private set; }

    public string? PhoneNumberE164 { get; private set; }

    public CriticalNotificationSmsFallbackStatus Status { get; private set; }

    public int RetryCount { get; private set; }

    public DateTime? LastAttemptAtUtc { get; private set; }

    public DateTime? NextRetryAtUtc { get; private set; }

    public DateTime? FinalizedAtUtc { get; private set; }

    public static CriticalNotificationSmsFallback CreatePending(
        Guid recipientId,
        Guid? userId,
        string recipientType,
        string eventType,
        string title,
        string body,
        string payloadJson,
        string? email,
        string? phoneNumberE164,
        DateTime scheduledAtUtc)
    {
        ValidateRecipient(recipientId, recipientType, eventType, title, body, scheduledAtUtc);

        return new CriticalNotificationSmsFallback
        {
            Id = Guid.CreateVersion7(),
            RecipientId = recipientId,
            UserId = userId,
            RecipientType = recipientType.Trim().ToLowerInvariant(),
            EventType = eventType.Trim(),
            Title = title.Trim(),
            Body = body.Trim(),
            PayloadJson = string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson,
            Email = email,
            PhoneNumberE164 = phoneNumberE164,
            Status = CriticalNotificationSmsFallbackStatus.AwaitingProcessing,
            RetryCount = 0,
            NextRetryAtUtc = scheduledAtUtc
        };
    }

    public bool IsDue(DateTime asOfUtc) =>
        Status is CriticalNotificationSmsFallbackStatus.AwaitingProcessing
            or CriticalNotificationSmsFallbackStatus.AwaitingRetry
        && NextRetryAtUtc.HasValue
        && NextRetryAtUtc.Value <= asOfUtc;

    public void RecordSuccessfulAttempt(DateTime attemptedAtUtc)
    {
        EnsureUtc(attemptedAtUtc, nameof(attemptedAtUtc));
        Status = CriticalNotificationSmsFallbackStatus.Sent;
        LastAttemptAtUtc = attemptedAtUtc;
        NextRetryAtUtc = null;
        FinalizedAtUtc = attemptedAtUtc;
        Touch();
    }

    public void RecordFailedAttempt(
        DateTime attemptedAtUtc,
        int maxRetries,
        TimeSpan retryInterval)
    {
        EnsureUtc(attemptedAtUtc, nameof(attemptedAtUtc));
        if (maxRetries < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRetries), "At least one retry must be allowed.");
        }

        RetryCount++;
        LastAttemptAtUtc = attemptedAtUtc;

        if (RetryCount >= maxRetries)
        {
            Status = CriticalNotificationSmsFallbackStatus.FailedFinal;
            NextRetryAtUtc = null;
            FinalizedAtUtc = attemptedAtUtc;
        }
        else
        {
            Status = CriticalNotificationSmsFallbackStatus.AwaitingRetry;
            NextRetryAtUtc = attemptedAtUtc.Add(retryInterval);
        }

        Touch();
    }

    private static void ValidateRecipient(
        Guid recipientId,
        string recipientType,
        string eventType,
        string title,
        string body,
        DateTime scheduledAtUtc)
    {
        if (recipientId == Guid.Empty)
        {
            throw new ArgumentException("Recipient id is required.", nameof(recipientId));
        }

        if (string.IsNullOrWhiteSpace(recipientType))
        {
            throw new ArgumentException("Recipient type is required.", nameof(recipientType));
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("Event type is required.", nameof(eventType));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("Body is required.", nameof(body));
        }

        EnsureUtc(scheduledAtUtc, nameof(scheduledAtUtc));
    }

    private static void EnsureUtc(DateTime timestamp, string parameterName)
    {
        if (timestamp.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamp must be UTC.", parameterName);
        }
    }
}
