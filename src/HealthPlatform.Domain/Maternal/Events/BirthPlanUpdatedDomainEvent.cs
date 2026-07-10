using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Maternal.Events;

public sealed record BirthPlanUpdatedDomainEvent(
    Guid BirthPlanId,
    Guid AntenatalRecordId,
    Guid PatientId,
    Guid ObstetricDoctorId,
    DateTime UpdatedAtUtc) : DomainEvent;
