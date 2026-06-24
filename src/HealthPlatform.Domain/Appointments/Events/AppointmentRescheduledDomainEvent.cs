using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Appointments.Events;

public sealed record AppointmentRescheduledDomainEvent(
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    Guid PreviousSlotId,
    DateTime PreviousScheduledAtUtc,
    Guid NewSlotId,
    DateTime NewScheduledAtUtc,
    DateTime RescheduledAtUtc) : DomainEvent;
