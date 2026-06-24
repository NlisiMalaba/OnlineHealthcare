using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Appointments.Events;

public sealed record AppointmentRefundRequestedDomainEvent(
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    Guid SlotId) : DomainEvent;
