using HealthPlatform.Application.Payments;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingPaymentFailedNotifier : IPaymentFailedNotifier
{
    public List<PaymentFailedNotificationRecord> Notifications { get; } = [];

    public Task NotifyPaymentFailedAsync(
        Guid patientUserId,
        Guid patientId,
        Guid paymentId,
        Guid? appointmentId,
        Guid? medicationOrderId,
        string failureCode,
        string failureMessage,
        DateTime retentionExpiresAtUtc,
        CancellationToken ct)
    {
        Notifications.Add(new PaymentFailedNotificationRecord(
            patientUserId,
            patientId,
            paymentId,
            appointmentId,
            medicationOrderId,
            failureCode,
            failureMessage,
            retentionExpiresAtUtc));

        return Task.CompletedTask;
    }
}

public sealed record PaymentFailedNotificationRecord(
    Guid PatientUserId,
    Guid PatientId,
    Guid PaymentId,
    Guid? AppointmentId,
    Guid? MedicationOrderId,
    string FailureCode,
    string FailureMessage,
    DateTime RetentionExpiresAtUtc);
