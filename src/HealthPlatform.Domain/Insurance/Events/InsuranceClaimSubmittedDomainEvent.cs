using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Insurance.Events;

public sealed record InsuranceClaimSubmittedDomainEvent(
    Guid ClaimId,
    Guid PatientId,
    string InsurerCode) : DomainEvent;
