using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Identity;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Appointments;

public sealed class AppointmentReminderDispatcher(
    TimeProvider timeProvider,
    IAppointmentRepository appointmentRepository,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IAppointmentReminderNotifier notifier,
    ILogger<AppointmentReminderDispatcher> logger) : IAppointmentReminderDispatcher
{
    public async Task<int> DispatchDueRemindersAsync(CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var dueAppointments = await appointmentRepository.ListConfirmedDueForReminderAsync(
            now,
            AppointmentPolicies.ReminderLeadTime,
            ct);

        if (dueAppointments.Count == 0)
        {
            return 0;
        }

        var dispatched = 0;
        foreach (var appointment in dueAppointments)
        {
            ct.ThrowIfCancellationRequested();

            var patient = await patientRepository.GetByIdAsync(appointment.PatientId, ct);
            if (patient is null)
            {
                logger.LogWarning(
                    "Skipping appointment reminder for {AppointmentId}; patient {PatientId} was not found.",
                    appointment.Id,
                    appointment.PatientId);
                continue;
            }

            var doctor = await doctorRepository.GetByIdAsync(appointment.DoctorId, ct);
            if (doctor is null)
            {
                logger.LogWarning(
                    "Skipping appointment reminder for {AppointmentId}; doctor {DoctorId} was not found.",
                    appointment.Id,
                    appointment.DoctorId);
                continue;
            }

            await notifier.NotifyAppointmentReminderAsync(
                patient.UserId,
                doctor.UserId,
                appointment.Id,
                appointment.ScheduledAtUtc,
                ct);

            if (!appointment.MarkReminderSent(now))
            {
                continue;
            }

            await appointmentRepository.UpdateAsync(appointment, ct);
            dispatched++;

            logger.LogInformation(
                "Dispatched appointment reminder for appointment {AppointmentId} scheduled at {ScheduledAtUtc}.",
                appointment.Id,
                appointment.ScheduledAtUtc);
        }

        return dispatched;
    }
}
