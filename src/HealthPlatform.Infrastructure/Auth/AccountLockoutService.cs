using HealthPlatform.Application.Auth;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Identity.Events;
using HealthPlatform.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace HealthPlatform.Infrastructure.Auth;

public sealed class AccountLockoutService(
    UserManager<ApplicationUser> userManager,
    IOutboxRepository outboxRepository) : IAccountLockoutService
{
    public async Task RecordNewLockoutIfNeededAsync(Guid userId, bool wasLockedBeforeAttempt, CancellationToken ct)
    {
        if (wasLockedBeforeAttempt)
        {
            return;
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null || !await userManager.IsLockedOutAsync(user))
        {
            return;
        }

        var lockoutEnd = user.LockoutEnd ?? DateTimeOffset.UtcNow;
        var domainEvent = new AccountLockedDomainEvent(user.Id, lockoutEnd, user.AccessFailedCount);
        await outboxRepository.EnqueueAsync(domainEvent, ct);
    }
}
