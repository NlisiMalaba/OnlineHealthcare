using HealthPlatform.Application.Wellness;
using HealthPlatform.Domain.Wellness;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class ConsecutiveMissedDoseAlertRepository(ApplicationDbContext db) : IConsecutiveMissedDoseAlertRepository
{
    public Task<bool> ExistsForStreakAsync(
        Guid patientId,
        DateTime streakEndScheduledAtUtc,
        CancellationToken ct) =>
        db.ConsecutiveMissedDoseAlerts.AnyAsync(
            alert => alert.PatientId == patientId
                && alert.StreakEndScheduledAtUtc == streakEndScheduledAtUtc,
            ct);

    public async Task AddAsync(ConsecutiveMissedDoseAlert alert, CancellationToken ct) =>
        await db.ConsecutiveMissedDoseAlerts.AddAsync(alert, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
