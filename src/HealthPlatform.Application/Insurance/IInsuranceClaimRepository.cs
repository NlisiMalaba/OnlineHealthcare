using HealthPlatform.Domain.Insurance;

namespace HealthPlatform.Application.Insurance;

public interface IInsuranceClaimRepository
{
    Task<InsuranceClaim?> GetByIdAsync(Guid claimId, CancellationToken ct);

    Task<InsuranceClaim?> GetByIdForPatientAsync(Guid claimId, Guid patientId, CancellationToken ct);

    Task<InsuranceClaim?> GetByInsurerReferenceAsync(string insurerCode, string insurerClaimReference, CancellationToken ct);

    Task<bool> ExistsForTargetAsync(
        Guid patientId,
        InsuranceClaimType claimType,
        Guid? appointmentId,
        Guid? medicationOrderId,
        Guid? labOrderId,
        CancellationToken ct);

    Task<IReadOnlyList<InsuranceClaim>> ListForPatientAsync(Guid patientId, CancellationToken ct);

    Task<IReadOnlyList<InsuranceClaim>> ListPendingStatusChecksAsync(
        DateTime checkedBeforeUtc,
        int take,
        CancellationToken ct);

    Task AddAsync(InsuranceClaim claim, CancellationToken ct);

    Task UpdateAsync(InsuranceClaim claim, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
