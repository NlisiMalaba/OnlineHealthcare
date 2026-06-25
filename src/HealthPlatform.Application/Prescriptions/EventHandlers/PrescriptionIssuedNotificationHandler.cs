using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Prescriptions.Notifications;
using MediatR;

namespace HealthPlatform.Application.Prescriptions.EventHandlers;

public sealed class PrescriptionIssuedNotificationHandler(
    IPatientRepository patientRepository,
    IPrescriptionIssuedNotifier notifier)
    : INotificationHandler<PrescriptionIssuedNotification>
{
    public async Task Handle(PrescriptionIssuedNotification notification, CancellationToken ct)
    {
        var patient = await patientRepository.GetByIdAsync(notification.PatientId, ct)
            ?? throw new NotFoundException(
                PrescriptionErrorCodes.PatientNotFound,
                "Patient profile was not found.");

        await notifier.NotifyPrescriptionIssuedAsync(
            patient.UserId,
            notification.PrescriptionId,
            notification.IssuedAtUtc,
            ct);
    }
}
