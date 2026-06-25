using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Prescriptions.Events;

public sealed record PrescriptionIssuedDomainEvent(
    Guid PrescriptionId,
    Guid DoctorId,
    Guid PatientId,
    Guid HealthRecordId,
    DateTime IssuedAtUtc,
    DateTime ExpiresAtUtc) : DomainEvent;
