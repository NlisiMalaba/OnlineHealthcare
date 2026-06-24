using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using MediatR;

namespace HealthPlatform.Application.Appointments.EventHandlers;

public sealed class AppointmentRescheduledNotificationHandler(
    IPatientRepository patientRepository,
    IAppointmentRescheduleNotifier notifier)
    : INotificationHandler<AppointmentRescheduledNotification>
{
    public async Task Handle(AppointmentRescheduledNotification notification, CancellationToken ct)
    {
        var patient = await patientRepository.GetByIdAsync(notification.PatientId, ct)
            ?? throw new NotFoundException("PATIENT_NOT_FOUND", "Patient profile was not found.");

        await notifier.NotifyAppointmentRescheduledAsync(
            patient.UserId,
            notification.AppointmentId,
            notification.PreviousScheduledAtUtc,
            notification.NewScheduledAtUtc,
            ct);
    }
}
