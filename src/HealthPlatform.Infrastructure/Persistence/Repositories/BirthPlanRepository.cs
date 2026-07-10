using HealthPlatform.Application.Maternal.BirthPlans;
using HealthPlatform.Domain.Maternal;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class BirthPlanRepository(ApplicationDbContext db) : IBirthPlanRepository
{
    public async Task AddAsync(BirthPlan birthPlan, CancellationToken ct)
    {
        await db.BirthPlans.AddAsync(birthPlan, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task<BirthPlan?> GetByIdAsync(Guid birthPlanId, CancellationToken ct) =>
        db.BirthPlans.SingleOrDefaultAsync(plan => plan.Id == birthPlanId, ct);

    public Task<BirthPlan?> GetByAntenatalRecordIdAsync(Guid antenatalRecordId, CancellationToken ct) =>
        db.BirthPlans.SingleOrDefaultAsync(plan => plan.AntenatalRecordId == antenatalRecordId, ct);

    public Task UpdateAsync(BirthPlan birthPlan, CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
