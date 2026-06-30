using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Payments.Instalments.Notifications;
using MediatR;

namespace HealthPlatform.Application.Payments.Instalments.EventHandlers;

public sealed class InstalmentPaymentMissedNotificationHandler(
    IPatientRepository patientRepository,
    IInstalmentMissedPaymentNotifier missedPaymentNotifier)
    : INotificationHandler<InstalmentPaymentMissedNotification>
{
    public async Task Handle(InstalmentPaymentMissedNotification notification, CancellationToken ct)
    {
        var patient = await patientRepository.GetByIdAsync(notification.PatientId, ct)
            ?? throw new NotFoundException("PATIENT_NOT_FOUND", "Patient profile was not found.");

        await missedPaymentNotifier.NotifyMissedPaymentAsync(
            patient.UserId,
            notification.PatientId,
            notification.InstalmentPlanId,
            notification.InstalmentPaymentId,
            notification.SequenceNumber,
            notification.AmountMinorUnits,
            notification.LateFeeMinorUnits,
            notification.Currency,
            notification.DueDate,
            ct);
    }
}
