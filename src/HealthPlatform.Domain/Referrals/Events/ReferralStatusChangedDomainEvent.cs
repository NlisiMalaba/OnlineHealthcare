using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Referrals.Events;

public sealed record ReferralStatusChangedDomainEvent(
    Guid ReferralId,
    Guid PatientId,
    Guid ReferringDoctorId,
    Guid? ReceivingDoctorId,
    ReferralStatus Status,
    string? Reason,
    DateTime RespondedAtUtc) : DomainEvent;
