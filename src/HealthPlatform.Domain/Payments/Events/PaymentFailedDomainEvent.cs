using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Payments.Events;

public sealed record PaymentFailedDomainEvent(
    Guid PaymentId,
    Guid PatientId,
    Guid? AppointmentId,
    Guid? MedicationOrderId,
    Guid? LabOrderId,
    string FailureCode,
    string FailureMessage,
    DateTime RetentionExpiresAtUtc) : DomainEvent;
