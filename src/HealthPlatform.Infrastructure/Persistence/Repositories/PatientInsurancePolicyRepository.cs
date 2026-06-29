using HealthPlatform.Application.Insurance;
using HealthPlatform.Domain.Insurance;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class PatientInsurancePolicyRepository(ApplicationDbContext db) : IPatientInsurancePolicyRepository
{
    public Task<PatientInsurancePolicy?> GetActiveByPatientAndInsurerAsync(
        Guid patientId,
        string insurerCode,
        DateOnly asOfDate,
        CancellationToken ct) =>
        db.PatientInsurancePolicies
            .Where(policy => policy.PatientId == patientId
                             && policy.InsurerCode == insurerCode
                             && policy.IsActive
                             && policy.ValidFrom <= asOfDate
                             && (policy.ValidTo == null || policy.ValidTo >= asOfDate))
            .OrderByDescending(policy => policy.ValidFrom)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(PatientInsurancePolicy policy, CancellationToken ct)
    {
        await db.PatientInsurancePolicies.AddAsync(policy, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
