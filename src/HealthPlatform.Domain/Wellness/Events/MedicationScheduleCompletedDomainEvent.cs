using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Wellness.Events;

public sealed record MedicationScheduleCompletedDomainEvent(
    Guid ScheduleId,
    Guid PrescriptionId,
    Guid PatientId,
    string MedicationName,
    DateTime CompletedAtUtc) : DomainEvent;
