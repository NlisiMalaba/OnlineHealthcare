using HealthPlatform.Domain.Notifications;

namespace HealthPlatform.Application.Notifications;

public interface INotificationPreferenceRepository
{
    Task<IReadOnlyList<UserNotificationPreference>> ListByUserIdAsync(Guid userId, CancellationToken ct);

    Task<IReadOnlyList<UserNotificationPreference>> ListByUserIdAndEventTypesAsync(
        Guid userId,
        IReadOnlyList<string> eventTypes,
        CancellationToken ct);

    Task AddRangeAsync(IReadOnlyList<UserNotificationPreference> preferences, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
