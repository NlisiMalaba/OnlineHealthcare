using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Identity.Events;

public sealed record DoctorProfileUpdatedDomainEvent(Guid DoctorId) : DomainEvent;
