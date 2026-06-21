using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Identity.Events;

public sealed record PharmacyRegisteredDomainEvent(
    Guid PharmacyId,
    string Name,
    string ContactEmail) : DomainEvent;
