namespace HealthPlatform.Application.Identity;

public interface IAccountLockoutNotifier
{
    Task NotifyAccountLockedAsync(Guid userId, DateTimeOffset lockoutEndUtc, CancellationToken ct);
}
