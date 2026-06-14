using MediatR;

namespace HealthPlatform.Application.Identity.Notifications;

public sealed record AccountLockedNotification(
    Guid UserId,
    DateTimeOffset LockoutEndUtc,
    int FailedAttemptCount) : INotification;
