using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Prescriptions.Notifications;
using MediatR;

namespace HealthPlatform.Application.Prescriptions.EventHandlers;

public sealed class DrugInteractionAlertDetectedNotificationHandler(
    IDoctorRepository doctorRepository,
    IDrugInteractionAlertNotifier notifier)
    : INotificationHandler<DrugInteractionAlertDetectedNotification>
{
    public async Task Handle(DrugInteractionAlertDetectedNotification notification, CancellationToken ct)
    {
        var doctor = await doctorRepository.GetByIdAsync(notification.DoctorId, ct)
            ?? throw new NotFoundException(
                PrescriptionErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");

        await notifier.NotifyDrugInteractionAlertAsync(
            doctor.UserId,
            notification.PatientId,
            notification.ProposedMedicationName,
            notification.InteractingMedicationName,
            notification.InteractionDescription,
            ct);
    }
}
