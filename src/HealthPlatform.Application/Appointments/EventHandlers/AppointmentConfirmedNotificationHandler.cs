using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using MediatR;

namespace HealthPlatform.Application.Appointments.EventHandlers;

public sealed class AppointmentConfirmedNotificationHandler(
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IAppointmentConfirmationNotifier notifier)
    : INotificationHandler<AppointmentConfirmedNotification>
{
    public async Task Handle(AppointmentConfirmedNotification notification, CancellationToken ct)
    {
        var patient = await patientRepository.GetByIdAsync(notification.PatientId, ct)
            ?? throw new NotFoundException("PATIENT_NOT_FOUND", "Patient profile was not found.");

        var doctor = await doctorRepository.GetByIdAsync(notification.DoctorId, ct)
            ?? throw new NotFoundException("DOCTOR_NOT_FOUND", "Doctor profile was not found.");

        await notifier.NotifyAppointmentConfirmedAsync(
            patient.UserId,
            doctor.UserId,
            notification.AppointmentId,
            notification.ScheduledAtUtc,
            ct);
    }
}
