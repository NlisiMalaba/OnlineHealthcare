using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Identity.Events;

public sealed record DoctorAvailabilityChangedDomainEvent(Guid DoctorId) : DomainEvent;
