using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Prescriptions;
using HealthPlatform.Application.Wellness.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Wellness.EventHandlers;

public sealed class NotifyPartiesOnMedicationScheduleCompletedNotificationHandler(
    IPrescriptionRepository prescriptionRepository,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IMedicationScheduleCompletionNotifier notifier,
    ILogger<NotifyPartiesOnMedicationScheduleCompletedNotificationHandler> logger)
    : INotificationHandler<MedicationScheduleCompletedNotification>
{
    public async Task Handle(MedicationScheduleCompletedNotification notification, CancellationToken ct)
    {
        var prescription = await prescriptionRepository.GetByIdAsync(notification.PrescriptionId, ct);
        if (prescription is null)
        {
            logger.LogWarning(
                "Cannot notify schedule completion {ScheduleId}: prescription {PrescriptionId} not found.",
                notification.ScheduleId,
                notification.PrescriptionId);
            return;
        }

        var patient = await patientRepository.GetByIdAsync(notification.PatientId, ct);
        var doctor = await doctorRepository.GetByIdAsync(prescription.DoctorId, ct);
        if (patient is null || doctor is null)
        {
            logger.LogWarning(
                "Cannot notify schedule completion {ScheduleId}: patient or doctor not found.",
                notification.ScheduleId);
            return;
        }

        await notifier.NotifyScheduleCompletedAsync(
            new MedicationScheduleCompletionNotice(
                notification.ScheduleId,
                notification.PrescriptionId,
                patient.Id,
                patient.UserId,
                doctor.Id,
                doctor.UserId,
                notification.MedicationName,
                notification.CompletedAtUtc),
            ct);

        logger.LogInformation(
            "Dispatched schedule completion notification for schedule {ScheduleId} to patient and doctor.",
            notification.ScheduleId);
    }
}
