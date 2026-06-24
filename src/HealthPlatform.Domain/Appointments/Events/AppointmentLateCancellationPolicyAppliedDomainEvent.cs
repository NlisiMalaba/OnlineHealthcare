using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Appointments.Events;

public sealed record AppointmentLateCancellationPolicyAppliedDomainEvent(
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    decimal RetentionPercent) : DomainEvent;
