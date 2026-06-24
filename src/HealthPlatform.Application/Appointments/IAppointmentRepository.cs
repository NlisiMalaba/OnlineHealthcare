using HealthPlatform.Domain.Appointments;

namespace HealthPlatform.Application.Appointments;

public interface IAppointmentRepository
{
    Task AddAsync(Appointment appointment, CancellationToken ct);

    Task<Appointment?> GetByIdAsync(Guid appointmentId, CancellationToken ct);

    Task<Appointment?> GetByIdForPatientAsync(Guid appointmentId, Guid patientId, CancellationToken ct);

    Task UpdateAsync(Appointment appointment, CancellationToken ct);

    Task<IReadOnlyList<Appointment>> ListConfirmedDueForReminderAsync(
        DateTime asOfUtc,
        TimeSpan reminderLeadTime,
        CancellationToken ct);
}
