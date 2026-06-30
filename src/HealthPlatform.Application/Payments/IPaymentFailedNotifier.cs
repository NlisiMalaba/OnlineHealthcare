namespace HealthPlatform.Application.Payments;

public interface IPaymentFailedNotifier
{
    Task NotifyPaymentFailedAsync(
        Guid patientUserId,
        Guid patientId,
        Guid paymentId,
        Guid? appointmentId,
        Guid? medicationOrderId,
        string failureCode,
        string failureMessage,
        DateTime retentionExpiresAtUtc,
        CancellationToken ct);
}
