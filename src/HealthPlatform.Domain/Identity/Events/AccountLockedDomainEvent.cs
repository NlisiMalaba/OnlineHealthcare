using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Identity.Events;

public sealed record AccountLockedDomainEvent(
    Guid UserId,
    DateTimeOffset LockoutEndUtc,
    int FailedAttemptCount) : DomainEvent;
