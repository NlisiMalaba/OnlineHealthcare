using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Notifications;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingAccountLockoutNotifier(INotificationDispatcher dispatcher)
    : IAccountLockoutNotifier
{
    public Task NotifyAccountLockedAsync(Guid userId, DateTimeOffset lockoutEndUtc, CancellationToken ct) =>
        NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            userId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.AccountLocked,
            NotificationCriticality.Critical,
            "Account locked",
            "Your account has been temporarily locked due to failed login attempts.",
            new Dictionary<string, string>
            {
                ["lockout_end_utc"] = lockoutEndUtc.ToString("O")
            },
            ct);
}
