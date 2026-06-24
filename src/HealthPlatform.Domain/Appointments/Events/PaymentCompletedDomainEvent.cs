using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Appointments.Events;

public sealed record PaymentCompletedDomainEvent(
    Guid AppointmentId,
    Guid PaymentId) : DomainEvent;
