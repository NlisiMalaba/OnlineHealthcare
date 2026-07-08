using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Notifications.GetNotificationPreferences;
using MediatR;

namespace HealthPlatform.Application.Notifications.GetNotificationPreferences;

public sealed class GetNotificationPreferencesQueryHandler(
    ICurrentUserAccessor currentUser,
    IUserRoleResolver userRoleResolver,
    INotificationPreferenceService preferenceService)
    : IRequestHandler<GetNotificationPreferencesQuery, NotificationPreferencesDto>
{
    public async Task<NotificationPreferencesDto> Handle(GetNotificationPreferencesQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var roles = await userRoleResolver.GetRolesAsync(userId, ct);
        if (roles.Count == 0)
        {
            throw new NotFoundException("USER_NOT_FOUND", "User account was not found.");
        }

        return await preferenceService.GetPreferencesForRolesAsync(userId, roles, ct);
    }
}
