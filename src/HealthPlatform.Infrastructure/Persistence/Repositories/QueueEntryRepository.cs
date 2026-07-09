using HealthPlatform.Application.Queue;
using HealthPlatform.Domain.Queue;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class QueueEntryRepository(ApplicationDbContext db) : IQueueEntryRepository
{
    public async Task AddAsync(QueueEntry entry, CancellationToken ct)
    {
        await db.QueueEntries.AddAsync(entry, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task<bool> ExistsActiveForAppointmentAsync(Guid appointmentId, CancellationToken ct) =>
        db.QueueEntries.AnyAsync(
            entry => entry.AppointmentId == appointmentId
                && (entry.ArrivalStatus == QueueArrivalStatus.NotArrived
                    || entry.ArrivalStatus == QueueArrivalStatus.Arrived
                    || entry.ArrivalStatus == QueueArrivalStatus.Called),
            ct);

    public Task UpdateAsync(QueueEntry entry, CancellationToken ct) =>
        db.SaveChangesAsync(ct);

    public async Task DeleteAsync(QueueEntry entry, CancellationToken ct)
    {
        db.QueueEntries.Remove(entry);
        await db.SaveChangesAsync(ct);
    }

    public Task<int> CountActiveByDoctorIdAsync(Guid doctorId, CancellationToken ct) =>
        db.QueueEntries.CountAsync(
            entry => entry.DoctorId == doctorId
                && (entry.ArrivalStatus == QueueArrivalStatus.NotArrived
                    || entry.ArrivalStatus == QueueArrivalStatus.Arrived
                    || entry.ArrivalStatus == QueueArrivalStatus.Called),
            ct);

    public async Task<IReadOnlyList<QueueEntry>> ListActiveAsync(CancellationToken ct) =>
        await db.QueueEntries
            .Where(entry =>
                entry.ArrivalStatus == QueueArrivalStatus.NotArrived
                || entry.ArrivalStatus == QueueArrivalStatus.Arrived
                || entry.ArrivalStatus == QueueArrivalStatus.Called)
            .OrderBy(entry => entry.DoctorId)
            .ThenBy(entry => entry.QueuePosition)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<QueueEntry>> ListActiveByDoctorIdAsync(Guid doctorId, CancellationToken ct) =>
        await db.QueueEntries
            .Where(entry =>
                entry.DoctorId == doctorId
                && (entry.ArrivalStatus == QueueArrivalStatus.NotArrived
                    || entry.ArrivalStatus == QueueArrivalStatus.Arrived
                    || entry.ArrivalStatus == QueueArrivalStatus.Called))
            .OrderBy(entry => entry.QueuePosition)
            .ToListAsync(ct);

    public Task<QueueEntry?> GetByIdAsync(Guid queueEntryId, CancellationToken ct) =>
        db.QueueEntries.SingleOrDefaultAsync(entry => entry.Id == queueEntryId, ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
