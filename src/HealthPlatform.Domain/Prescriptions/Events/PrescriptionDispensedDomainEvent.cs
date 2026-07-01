using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Prescriptions.Events;

public sealed record PrescriptionDispensedDomainEvent(
    Guid PrescriptionId,
    Guid PatientId,
    string MedicationName,
    string Dosage,
    string Frequency,
    int DurationDays,
    DateTime DispensedAtUtc) : DomainEvent;
