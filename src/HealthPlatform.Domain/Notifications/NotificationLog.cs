using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Notifications;

public sealed class NotificationLog : Entity
{
    private NotificationLog()
    {
        EventType = string.Empty;
        Channel = string.Empty;
        RecipientType = string.Empty;
        PayloadJson = "{}";
    }

    public Guid RecipientId { get; private set; }

    public string RecipientType { get; private set; }

    public string Channel { get; private set; }

    public string EventType { get; private set; }

    public string PayloadJson { get; private set; }

    public NotificationDeliveryStatus Status { get; private set; }

    public int Attempts { get; private set; }

    public DateTime SentAtUtc { get; private set; }

    public DateTime? DeliveredAtUtc { get; private set; }

    public string? FailureReason { get; private set; }

    public static NotificationLog RecordAttempt(
        Guid recipientId,
        string recipientType,
        string channel,
        string eventType,
        string payloadJson,
        NotificationDeliveryStatus status,
        int attempts,
        DateTime sentAtUtc,
        DateTime? deliveredAtUtc,
        string? failureReason)
    {
        if (recipientId == Guid.Empty)
        {
            throw new ArgumentException("Recipient id is required.", nameof(recipientId));
        }

        if (string.IsNullOrWhiteSpace(recipientType))
        {
            throw new ArgumentException("Recipient type is required.", nameof(recipientType));
        }

        if (string.IsNullOrWhiteSpace(channel))
        {
            throw new ArgumentException("Channel is required.", nameof(channel));
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException("Event type is required.", nameof(eventType));
        }

        if (attempts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(attempts), "Attempts must be at least 1.");
        }

        EnsureUtc(sentAtUtc, nameof(sentAtUtc));
        if (deliveredAtUtc.HasValue)
        {
            EnsureUtc(deliveredAtUtc.Value, nameof(deliveredAtUtc));
        }

        return new NotificationLog
        {
            Id = Guid.CreateVersion7(),
            RecipientId = recipientId,
            RecipientType = recipientType.Trim().ToLowerInvariant(),
            Channel = channel.Trim().ToLowerInvariant(),
            EventType = eventType.Trim(),
            PayloadJson = string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson,
            Status = status,
            Attempts = attempts,
            SentAtUtc = sentAtUtc,
            DeliveredAtUtc = deliveredAtUtc,
            FailureReason = failureReason
        };
    }

    private static void EnsureUtc(DateTime timestamp, string parameterName)
    {
        if (timestamp.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamp must be UTC.", parameterName);
        }
    }
}
