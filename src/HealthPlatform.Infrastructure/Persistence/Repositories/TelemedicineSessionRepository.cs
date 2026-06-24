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

    public Task UpdateAsync(TelemedicineSession session, CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
