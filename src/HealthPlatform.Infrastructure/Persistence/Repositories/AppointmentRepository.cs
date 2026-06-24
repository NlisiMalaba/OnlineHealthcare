using HealthPlatform.Application.Appointments;
using HealthPlatform.Domain.Appointments;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class AppointmentRepository(ApplicationDbContext db) : IAppointmentRepository
{
    public async Task AddAsync(Appointment appointment, CancellationToken ct)
    {
        await db.Appointments.AddAsync(appointment, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task<Appointment?> GetByIdAsync(Guid appointmentId, CancellationToken ct) =>
        db.Appointments.SingleOrDefaultAsync(a => a.Id == appointmentId, ct);

    public Task<Appointment?> GetByIdForPatientAsync(Guid appointmentId, Guid patientId, CancellationToken ct) =>
        db.Appointments.SingleOrDefaultAsync(
            a => a.Id == appointmentId && a.PatientId == patientId,
            ct);

    public Task UpdateAsync(Appointment appointment, CancellationToken ct) =>
        db.SaveChangesAsync(ct);

    public async Task<IReadOnlyList<Appointment>> ListConfirmedDueForReminderAsync(
        DateTime asOfUtc,
        TimeSpan reminderLeadTime,
        CancellationToken ct)
    {
        var windowEnd = asOfUtc.Add(reminderLeadTime);

        return await db.Appointments
            .Where(a =>
                a.Status == AppointmentStatus.Confirmed
                && a.ReminderSentAtUtc == null
                && a.ScheduledAtUtc > asOfUtc
                && a.ScheduledAtUtc <= windowEnd)
            .OrderBy(a => a.ScheduledAtUtc)
            .ToListAsync(ct);
    }
}
