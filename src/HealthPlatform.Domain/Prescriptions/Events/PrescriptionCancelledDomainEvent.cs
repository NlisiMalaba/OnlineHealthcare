using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Prescriptions.Events;

public sealed record PrescriptionCancelledDomainEvent(
    Guid PrescriptionId,
    Guid DoctorId,
    Guid PatientId,
    string Reason,
    DateTime CancelledAtUtc) : DomainEvent;
