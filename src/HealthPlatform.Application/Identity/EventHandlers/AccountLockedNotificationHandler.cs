using HealthPlatform.Application.Identity.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Identity.EventHandlers;

public sealed class AccountLockedNotificationHandler(
    IAccountLockoutNotifier notifier,
    ILogger<AccountLockedNotificationHandler> logger) : INotificationHandler<AccountLockedNotification>
{
    public async Task Handle(AccountLockedNotification notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Account lockout notification dispatch for user {UserId} until {LockoutEndUtc}.",
            notification.UserId,
            notification.LockoutEndUtc);

        await notifier.NotifyAccountLockedAsync(notification.UserId, notification.LockoutEndUtc, ct);
    }
}
