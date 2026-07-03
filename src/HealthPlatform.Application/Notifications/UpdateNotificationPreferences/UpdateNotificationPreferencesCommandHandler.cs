using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Notifications;
using MediatR;

namespace HealthPlatform.Application.Notifications.UpdateNotificationPreferences;

public sealed class UpdateNotificationPreferencesCommandHandler(
    ICurrentUserAccessor currentUser,
    IUserRoleResolver userRoleResolver,
    INotificationPreferenceRepository repository,
    INotificationPreferenceCache cache,
    INotificationPreferenceService preferenceService)
    : IRequestHandler<UpdateNotificationPreferencesCommand, NotificationPreferencesDto>
{
    public async Task<NotificationPreferencesDto> Handle(
        UpdateNotificationPreferencesCommand request,
        CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var roles = await userRoleResolver.GetRolesAsync(userId, ct);
        if (roles.Count == 0)
        {
            throw new NotFoundException("USER_NOT_FOUND", "User account was not found.");
        }

        var configurableEventTypes = NotificationPreferenceCatalog.GetConfigurableEventTypes(roles);
        ValidateRequestedEventTypes(request.Preferences, configurableEventTypes);

        var eventTypes = request.Preferences
            .Select(preference => preference.EventType)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var existing = await repository.ListByUserIdAndEventTypesAsync(userId, eventTypes, ct);
        var existingByKey = existing.ToDictionary(
            preference => CreateKey(preference.EventType, preference.Channel),
            StringComparer.Ordinal);

        var toAdd = new List<UserNotificationPreference>();
        foreach (var preferenceUpdate in request.Preferences)
        {
            foreach (var channelUpdate in preferenceUpdate.Channels)
            {
                var channelKey = channelUpdate.Channel.ToLowerInvariant();
                var key = CreateKey(preferenceUpdate.EventType, channelKey);
                if (existingByKey.TryGetValue(key, out var existingPreference))
                {
                    existingPreference.SetEnabled(channelUpdate.IsEnabled);
                    continue;
                }

                toAdd.Add(UserNotificationPreference.Create(
                    userId,
                    preferenceUpdate.EventType,
                    channelKey,
                    channelUpdate.IsEnabled));
            }
        }

        if (toAdd.Count > 0)
        {
            await repository.AddRangeAsync(toAdd, ct);
        }

        await repository.SaveChangesAsync(ct);
        await cache.InvalidateAsync(userId, ct);

        return await preferenceService.GetPreferencesForRolesAsync(userId, roles, ct);
    }

    private static void ValidateRequestedEventTypes(
        IReadOnlyList<NotificationEventPreferenceUpdateDto> preferences,
        IReadOnlyList<string> configurableEventTypes)
    {
        foreach (var preference in preferences)
        {
            if (!configurableEventTypes.Contains(preference.EventType, StringComparer.Ordinal))
            {
                throw new DomainException(
                    "NOTIFICATION_PREFERENCE_NOT_CONFIGURABLE",
                    $"Notification event '{preference.EventType}' cannot be configured for this account.");
            }
        }
    }

    private static string CreateKey(string eventType, string channel) => $"{eventType}:{channel}";
}
