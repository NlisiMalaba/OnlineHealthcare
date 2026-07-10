using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Maternal.Events;

public sealed record AntenatalRecordCreatedDomainEvent(
    Guid AntenatalRecordId,
    Guid PatientId,
    Guid ObstetricDoctorId,
    DateOnly EstimatedDueDate,
    int GestationalAgeWeeks,
    DateTime CreatedAtUtc) : DomainEvent;
