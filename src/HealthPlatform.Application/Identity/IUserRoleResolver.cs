namespace HealthPlatform.Application.Identity;

public interface IUserRoleResolver
{
    Task<IReadOnlyList<string>> GetRolesAsync(Guid userId, CancellationToken ct);
}
