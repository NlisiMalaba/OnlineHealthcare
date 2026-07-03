using HealthPlatform.Application.Notifications;
using HealthPlatform.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace HealthPlatform.Infrastructure.Notifications;

public sealed class IdentityNotificationRecipientResolver(
    UserManager<ApplicationUser> userManager) : INotificationRecipientResolver
{
    public async Task<ResolvedNotificationRecipient> ResolveAsync(
        Guid userId,
        NotificationRecipientType recipientType,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString());
        return new ResolvedNotificationRecipient(
            userId,
            recipientType,
            user?.Email,
            user?.PhoneNumber,
            []);
    }
}
