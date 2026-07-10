using HealthPlatform.Domain.Maternal;

namespace HealthPlatform.Application.Maternal.BirthPlans;

public interface IBirthPlanRepository
{
    Task AddAsync(BirthPlan birthPlan, CancellationToken ct);

    Task<BirthPlan?> GetByIdAsync(Guid birthPlanId, CancellationToken ct);

    Task<BirthPlan?> GetByAntenatalRecordIdAsync(Guid antenatalRecordId, CancellationToken ct);

    Task UpdateAsync(BirthPlan birthPlan, CancellationToken ct);
}
