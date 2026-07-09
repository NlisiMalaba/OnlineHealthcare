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

    public Task<int> CountActiveByDoctorIdAsync(Guid doctorId, CancellationToken ct) =>
        db.QueueEntries.CountAsync(
            entry => entry.DoctorId == doctorId
                && (entry.ArrivalStatus == QueueArrivalStatus.NotArrived
                    || entry.ArrivalStatus == QueueArrivalStatus.Arrived
                    || entry.ArrivalStatus == QueueArrivalStatus.Called),
            ct);
}
