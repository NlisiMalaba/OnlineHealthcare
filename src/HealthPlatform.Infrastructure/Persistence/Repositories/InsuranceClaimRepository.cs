using HealthPlatform.Application.Insurance;
using HealthPlatform.Domain.Insurance;
using HealthPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class InsuranceClaimRepository(ApplicationDbContext db) : IInsuranceClaimRepository
{
    public Task<InsuranceClaim?> GetByIdAsync(Guid claimId, CancellationToken ct) =>
        db.InsuranceClaims.FirstOrDefaultAsync(c => c.Id == claimId, ct);

    public Task<InsuranceClaim?> GetByIdForPatientAsync(Guid claimId, Guid patientId, CancellationToken ct) =>
        db.InsuranceClaims.FirstOrDefaultAsync(c => c.Id == claimId && c.PatientId == patientId, ct);

    public Task<InsuranceClaim?> GetByInsurerReferenceAsync(
        string insurerCode,
        string insurerClaimReference,
        CancellationToken ct) =>
        db.InsuranceClaims.FirstOrDefaultAsync(
            c => c.InsurerCode == insurerCode && c.InsurerClaimReference == insurerClaimReference,
            ct);

    public Task<bool> ExistsForTargetAsync(
        Guid patientId,
        InsuranceClaimType claimType,
        Guid? appointmentId,
        Guid? medicationOrderId,
        Guid? labOrderId,
        CancellationToken ct) =>
        db.InsuranceClaims.AnyAsync(
            c => c.PatientId == patientId
                 && c.ClaimType == claimType
                 && c.AppointmentId == appointmentId
                 && c.MedicationOrderId == medicationOrderId
                 && c.LabOrderId == labOrderId,
            ct);

    public async Task<IReadOnlyList<InsuranceClaim>> ListForPatientAsync(Guid patientId, CancellationToken ct) =>
        await db.InsuranceClaims
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<InsuranceClaim>> ListPendingStatusChecksAsync(
        DateTime checkedBeforeUtc,
        int take,
        CancellationToken ct) =>
        await db.InsuranceClaims
            .Where(c => c.InsurerClaimReference != null
                        && (c.Status == InsuranceClaimStatus.Submitted
                            || c.Status == InsuranceClaimStatus.Processing)
                        && (c.LastStatusCheckedAtUtc == null || c.LastStatusCheckedAtUtc < checkedBeforeUtc))
            .OrderBy(c => c.LastStatusCheckedAtUtc ?? c.SubmittedAtUtc)
            .Take(take)
            .ToListAsync(ct);

    public async Task AddAsync(InsuranceClaim claim, CancellationToken ct)
    {
        await db.InsuranceClaims.AddAsync(claim, ct);
    }

    public Task UpdateAsync(InsuranceClaim claim, CancellationToken ct)
    {
        db.InsuranceClaims.Update(claim);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
