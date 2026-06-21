using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Identity.Events;

public sealed record PatientRegisteredDomainEvent(Guid PatientId) : DomainEvent;
