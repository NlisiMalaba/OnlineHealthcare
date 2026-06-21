using HealthPlatform.Application.Identity;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Identity;

/// <summary>
/// Development-oriented stub for account lockout notifications; replace with email/SMS/push in production.
/// </summary>
public sealed class LoggingAccountLockoutNotifier(ILogger<LoggingAccountLockoutNotifier> logger)
    : IAccountLockoutNotifier
{
    public Task NotifyAccountLockedAsync(Guid userId, DateTimeOffset lockoutEndUtc, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Account lockout notification requested for user {UserId} until {LockoutEndUtc}.",
            userId,
            lockoutEndUtc);
        return Task.CompletedTask;
    }
}
