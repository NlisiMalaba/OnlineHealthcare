using HealthPlatform.Application.Identity;
using HealthPlatform.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace HealthPlatform.Infrastructure.Identity;

public sealed class IdentityUserRoleResolver(UserManager<ApplicationUser> userManager) : IUserRoleResolver
{
    public async Task<IReadOnlyList<string>> GetRolesAsync(Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return [];
        }

        var roles = await userManager.GetRolesAsync(user);
        return roles.ToList();
    }
}
