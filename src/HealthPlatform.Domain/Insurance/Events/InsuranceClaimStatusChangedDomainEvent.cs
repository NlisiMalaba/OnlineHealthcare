using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Insurance.Events;

public sealed record InsuranceClaimStatusChangedDomainEvent(
    Guid ClaimId,
    Guid PatientId,
    InsuranceClaimStatus PreviousStatus,
    InsuranceClaimStatus NewStatus,
    string? StatusReason) : DomainEvent;
