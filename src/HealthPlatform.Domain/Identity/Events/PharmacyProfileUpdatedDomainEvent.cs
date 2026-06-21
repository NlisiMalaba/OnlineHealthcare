using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Identity.Events;

public sealed record PharmacyProfileUpdatedDomainEvent(Guid PharmacyId) : DomainEvent;
