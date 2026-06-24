using HealthPlatform.Application.Telemedicine;
using HealthPlatform.Domain.Telemedicine;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class TelemedicineSessionRepository(ApplicationDbContext db) : ITelemedicineSessionRepository
{
    public async Task AddAsync(TelemedicineSession session, CancellationToken ct)
    {
        await db.TelemedicineSessions.AddAsync(session, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task<TelemedicineSession?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken ct) =>
        db.TelemedicineSessions.SingleOrDefaultAsync(s => s.AppointmentId == appointmentId, ct);

    public Task<TelemedicineSession?> GetByIdAsync(Guid sessionId, CancellationToken ct) =>
        db.TelemedicineSessions.SingleOrDefaultAsync(s => s.Id == sessionId, ct);

    public async Task<IReadOnlyList<TelemedicineSession>> ListActiveSessionsAsync(CancellationToken ct) =>
        await db.TelemedicineSessions
            .Where(s => s.Status == TelemedicineSessionStatus.Active && s.StartedAtUtc != null)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<TelemedicineSession>> ListSessionsWithPendingReconnectionGraceAsync(
        DateTime asOfUtc,
        TimeSpan gracePeriod,
        CancellationToken ct)
    {
        var graceStartedBefore = asOfUtc.Subtract(gracePeriod);

        return await db.TelemedicineSessions
            .Where(s =>
                s.Status == TelemedicineSessionStatus.Active
                && s.InterruptedAtUtc != null
                && s.InterruptedAtUtc <= graceStartedBefore)
            .ToListAsync(ct);
    }

    public Task UpdateAsync(TelemedicineSession session, CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
