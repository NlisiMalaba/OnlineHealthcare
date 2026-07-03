using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.NextOfKin;

public sealed class NextOfKinNotificationDelivery : Entity
{
    private NextOfKinNotificationDelivery()
    {
    }

    public NextOfKinNotificationType NotificationType { get; private set; }

    public Guid ReferenceId { get; private set; }

    public Guid PatientId { get; private set; }

    public Guid NextOfKinContactId { get; private set; }

    public NextOfKinNotificationChannel Channel { get; private set; }

    public NextOfKinNotificationDeliveryStatus Status { get; private set; }

    public int RetryCount { get; private set; }

    public DateTime? LastAttemptAtUtc { get; private set; }

    public DateTime? NextRetryAtUtc { get; private set; }

    public DateTime? FinalizedAtUtc { get; private set; }

    public static NextOfKinNotificationDelivery CreateAwaitingRetry(
        NextOfKinNotificationType notificationType,
        Guid referenceId,
        Guid patientId,
        Guid nextOfKinContactId,
        NextOfKinNotificationChannel channel,
        DateTime failedAtUtc,
        TimeSpan retryInterval)
    {
        ValidateIds(referenceId, patientId, nextOfKinContactId);
        EnsureUtc(failedAtUtc, nameof(failedAtUtc));

        return new NextOfKinNotificationDelivery
        {
            Id = Guid.CreateVersion7(),
            NotificationType = notificationType,
            ReferenceId = referenceId,
            PatientId = patientId,
            NextOfKinContactId = nextOfKinContactId,
            Channel = channel,
            Status = NextOfKinNotificationDeliveryStatus.AwaitingRetry,
            RetryCount = 0,
            LastAttemptAtUtc = failedAtUtc,
            NextRetryAtUtc = failedAtUtc.Add(retryInterval)
        };
    }

    public bool IsDueForRetry(DateTime asOfUtc) =>
        Status == NextOfKinNotificationDeliveryStatus.AwaitingRetry
        && NextRetryAtUtc.HasValue
        && NextRetryAtUtc.Value <= asOfUtc;

    public void RecordSuccessfulAttempt(DateTime attemptedAtUtc)
    {
        EnsureUtc(attemptedAtUtc, nameof(attemptedAtUtc));
        Status = NextOfKinNotificationDeliveryStatus.Sent;
        LastAttemptAtUtc = attemptedAtUtc;
        NextRetryAtUtc = null;
        FinalizedAtUtc = attemptedAtUtc;
        Touch();
    }

    public void RecordFailedRetryAttempt(
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
            Status = NextOfKinNotificationDeliveryStatus.FailedFinal;
            NextRetryAtUtc = null;
            FinalizedAtUtc = attemptedAtUtc;
        }
        else
        {
            Status = NextOfKinNotificationDeliveryStatus.AwaitingRetry;
            NextRetryAtUtc = attemptedAtUtc.Add(retryInterval);
        }

        Touch();
    }

    private static void ValidateIds(Guid referenceId, Guid patientId, Guid nextOfKinContactId)
    {
        if (referenceId == Guid.Empty)
        {
            throw new ArgumentException("Reference id is required.", nameof(referenceId));
        }

        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (nextOfKinContactId == Guid.Empty)
        {
            throw new ArgumentException("Next-of-kin contact id is required.", nameof(nextOfKinContactId));
        }
    }

    private static void EnsureUtc(DateTime timestamp, string parameterName)
    {
        if (timestamp.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamp must be UTC.", parameterName);
        }
    }
}
