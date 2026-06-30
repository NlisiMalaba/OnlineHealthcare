using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Payments.Notifications;
using MediatR;

namespace HealthPlatform.Application.Payments.EventHandlers;

public sealed class PaymentFailedNotificationHandler(
    IPatientRepository patientRepository,
    IPaymentFailedNotifier paymentFailedNotifier)
    : INotificationHandler<PaymentFailedNotification>
{
    public async Task Handle(PaymentFailedNotification notification, CancellationToken ct)
    {
        var patient = await patientRepository.GetByIdAsync(notification.PatientId, ct)
            ?? throw new NotFoundException("PATIENT_NOT_FOUND", "Patient profile was not found.");

        await paymentFailedNotifier.NotifyPaymentFailedAsync(
            patient.UserId,
            notification.PatientId,
            notification.PaymentId,
            notification.AppointmentId,
            notification.MedicationOrderId,
            notification.FailureCode,
            notification.FailureMessage,
            notification.RetentionExpiresAtUtc,
            ct);
    }
}
