using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Prescriptions.Notifications;
using MediatR;

namespace HealthPlatform.Application.Prescriptions.EventHandlers;

public sealed class PrescriptionCancelledNotificationHandler(
    IPatientRepository patientRepository,
    IPrescriptionCancelledNotifier notifier)
    : INotificationHandler<PrescriptionCancelledNotification>
{
    public async Task Handle(PrescriptionCancelledNotification notification, CancellationToken ct)
    {
        var patient = await patientRepository.GetByIdAsync(notification.PatientId, ct)
            ?? throw new NotFoundException(
                PrescriptionErrorCodes.PatientNotFound,
                "Patient profile was not found.");

        await notifier.NotifyPrescriptionCancelledAsync(
            patient.UserId,
            notification.PrescriptionId,
            ct);
    }
}
