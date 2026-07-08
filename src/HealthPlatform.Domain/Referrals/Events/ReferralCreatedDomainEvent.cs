using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Referrals.Events;

public sealed record ReferralCreatedDomainEvent(
    Guid ReferralId,
    Guid PatientId,
    Guid ReferringDoctorId,
    Guid? ReceivingDoctorId,
    string? ReceivingHospitalName,
    string Reason,
    DateTime PatientConsentAtUtc,
    DateTime CreatedAtUtc) : DomainEvent;
