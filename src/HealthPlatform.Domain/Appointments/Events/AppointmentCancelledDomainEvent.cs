using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Appointments.Events;

public sealed record AppointmentCancelledDomainEvent(
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    Guid SlotId,
    DateTime ScheduledAtUtc,
    DateTime CancelledAtUtc,
    bool IsEarlyCancellation) : DomainEvent;
