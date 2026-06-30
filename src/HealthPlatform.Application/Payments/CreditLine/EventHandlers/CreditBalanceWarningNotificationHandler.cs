using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Payments.CreditLine.Notifications;
using MediatR;

namespace HealthPlatform.Application.Payments.CreditLine.EventHandlers;

public sealed class CreditBalanceWarningNotificationHandler(
    IPatientRepository patientRepository,
    ICreditBalanceWarningNotifier balanceWarningNotifier)
    : INotificationHandler<CreditBalanceWarningNotification>
{
    public async Task Handle(CreditBalanceWarningNotification notification, CancellationToken ct)
    {
        var patient = await patientRepository.GetByIdAsync(notification.PatientId, ct)
            ?? throw new NotFoundException("PATIENT_NOT_FOUND", "Patient profile was not found.");

        await balanceWarningNotifier.NotifyBalanceWarningAsync(
            patient.UserId,
            notification.PatientId,
            notification.OutstandingBalanceMinorUnits,
            notification.CreditLimitMinorUnits,
            notification.Currency,
            ct);
    }
}
