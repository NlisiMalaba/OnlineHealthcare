using HealthPlatform.Application.MentalHealth;
using HealthPlatform.Domain.MentalHealth;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class TherapySessionRepository(ApplicationDbContext db) : ITherapySessionRepository
{
    public async Task AddAsync(TherapySession session, CancellationToken ct)
    {
        await db.TherapySessions.AddAsync(session, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task<TherapySession?> GetByIdAsync(Guid therapySessionId, CancellationToken ct) =>
        db.TherapySessions.SingleOrDefaultAsync(session => session.Id == therapySessionId, ct);

    public Task<TherapySession?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken ct) =>
        db.TherapySessions.SingleOrDefaultAsync(session => session.AppointmentId == appointmentId, ct);

    public Task UpdateAsync(TherapySession session, CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
