using HealthPlatform.Application.Wellness.HealthGoals;
using HealthPlatform.Domain.Wellness;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class HealthGoalRepository(ApplicationDbContext db) : IHealthGoalRepository
{
    public async Task<IReadOnlyList<HealthGoal>> ListByPatientIdAsync(
        Guid patientId,
        HealthGoalStatus? status,
        CancellationToken ct)
    {
        var query = db.HealthGoals
            .AsNoTracking()
            .Where(goal => goal.PatientId == patientId);

        if (status.HasValue)
        {
            query = query.Where(goal => goal.Status == status.Value);
        }

        return await query
            .OrderByDescending(goal => goal.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<HealthGoal>> ListActiveByPatientAndMetricAsync(
        Guid patientId,
        WellnessMetricType metricType,
        CancellationToken ct) =>
        await db.HealthGoals
            .AsNoTracking()
            .Where(goal =>
                goal.PatientId == patientId
                && goal.MetricType == metricType
                && goal.Status == HealthGoalStatus.Active)
            .OrderByDescending(goal => goal.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task<HealthGoal?> GetByIdForPatientAsync(Guid goalId, Guid patientId, CancellationToken ct) =>
        await db.HealthGoals
            .FirstOrDefaultAsync(goal => goal.Id == goalId && goal.PatientId == patientId, ct);

    public async Task AddAsync(HealthGoal goal, CancellationToken ct)
    {
        await db.HealthGoals.AddAsync(goal, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(HealthGoal goal, CancellationToken ct)
    {
        db.HealthGoals.Update(goal);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(HealthGoal goal, CancellationToken ct)
    {
        db.HealthGoals.Remove(goal);
        await db.SaveChangesAsync(ct);
    }
}
