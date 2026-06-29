using HealthPlatform.Domain.Insurance;

namespace HealthPlatform.Application.Insurance;

public sealed record InsuranceClaimDto(
    Guid Id,
    string InsurerCode,
    InsuranceClaimType ClaimType,
    InsuranceClaimStatus Status,
    long AmountMinorUnits,
    string Currency,
    Guid? AppointmentId,
    Guid? MedicationOrderId,
    Guid? LabOrderId,
    string? InsurerClaimReference,
    string? StatusReason,
    DateTime? SubmittedAtUtc,
    DateTime? LastStatusCheckedAtUtc,
    DateTime CreatedAtUtc);

public sealed record InsuranceClaimListItemDto(
    Guid Id,
    string InsurerCode,
    InsuranceClaimType ClaimType,
    InsuranceClaimStatus Status,
    long AmountMinorUnits,
    string Currency,
    DateTime? SubmittedAtUtc,
    DateTime CreatedAtUtc);
