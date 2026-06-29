using HealthPlatform.Domain.Insurance;

namespace HealthPlatform.Application.Insurance;

public interface IPatientInsurancePolicyRepository
{
    Task<PatientInsurancePolicy?> GetActiveByPatientAndInsurerAsync(
        Guid patientId,
        string insurerCode,
        DateOnly asOfDate,
        CancellationToken ct);

    Task AddAsync(PatientInsurancePolicy policy, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
