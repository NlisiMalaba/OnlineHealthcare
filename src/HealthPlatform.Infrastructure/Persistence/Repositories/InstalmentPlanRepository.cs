using HealthPlatform.Application.Payments.Instalments;
using HealthPlatform.Domain.Payments.Instalments;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class InstalmentPlanRepository(ApplicationDbContext db) : IInstalmentPlanRepository
{
    public Task<InstalmentPlan?> GetByIdAsync(Guid planId, CancellationToken ct) =>
        db.InstalmentPlans.FirstOrDefaultAsync(p => p.Id == planId, ct);

    public Task<InstalmentPlan?> GetByIdForPatientAsync(Guid planId, Guid patientId, CancellationToken ct) =>
        db.InstalmentPlans.FirstOrDefaultAsync(p => p.Id == planId && p.PatientId == patientId, ct);

    public async Task<IReadOnlyList<InstalmentPlan>> ListForPatientAsync(Guid patientId, CancellationToken ct) =>
        await db.InstalmentPlans
            .Where(p => p.PatientId == patientId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task AddAsync(InstalmentPlan plan, CancellationToken ct)
    {
        await db.InstalmentPlans.AddAsync(plan, ct);
    }

    public Task UpdateAsync(InstalmentPlan plan, CancellationToken ct)
    {
        db.InstalmentPlans.Update(plan);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
