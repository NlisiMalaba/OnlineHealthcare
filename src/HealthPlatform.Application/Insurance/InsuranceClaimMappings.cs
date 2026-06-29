using HealthPlatform.Domain.Insurance;

namespace HealthPlatform.Application.Insurance;

public static class InsuranceClaimMappings
{
    public static InsuranceClaimDto ToDto(this InsuranceClaim claim) =>
        new(
            claim.Id,
            claim.InsurerCode,
            claim.ClaimType,
            claim.Status,
            claim.AmountMinorUnits,
            claim.Currency,
            claim.AppointmentId,
            claim.MedicationOrderId,
            claim.LabOrderId,
            claim.InsurerClaimReference,
            claim.StatusReason,
            claim.SubmittedAtUtc,
            claim.LastStatusCheckedAtUtc,
            claim.CreatedAtUtc);

    public static InsuranceClaimListItemDto ToListItemDto(this InsuranceClaim claim) =>
        new(
            claim.Id,
            claim.InsurerCode,
            claim.ClaimType,
            claim.Status,
            claim.AmountMinorUnits,
            claim.Currency,
            claim.SubmittedAtUtc,
            claim.CreatedAtUtc);
}
