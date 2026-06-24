using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Appointments.Events;

public sealed record AppointmentConfirmedDomainEvent(
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    DateTime ScheduledAtUtc,
    DateTime ConfirmedAtUtc) : DomainEvent;
