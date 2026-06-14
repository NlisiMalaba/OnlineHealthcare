namespace HealthPlatform.Application.Auth;

public interface IAccountLockoutService
{
    /// <summary>
    /// When an account becomes locked after a failed login attempt, enqueues an outbox domain event for notification.
    /// </summary>
    Task RecordNewLockoutIfNeededAsync(Guid userId, bool wasLockedBeforeAttempt, CancellationToken ct);
}
