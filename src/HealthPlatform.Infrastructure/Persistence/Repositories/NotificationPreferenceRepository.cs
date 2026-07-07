using HealthPlatform.Application.Notifications;
using HealthPlatform.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class NotificationPreferenceRepository(ApplicationDbContext dbContext) : INotificationPreferenceRepository
{
    public async Task<IReadOnlyList<UserNotificationPreference>> ListByUserIdAsync(
        Guid userId,
        CancellationToken ct) =>
        await dbContext.UserNotificationPreferences
            .Where(preference => preference.UserId == userId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<UserNotificationPreference>> ListByUserIdAndEventTypesAsync(
        Guid userId,
        IReadOnlyList<string> eventTypes,
        CancellationToken ct) =>
        await dbContext.UserNotificationPreferences
            .Where(preference => preference.UserId == userId && eventTypes.Contains(preference.EventType))
            .ToListAsync(ct);

    public async Task AddRangeAsync(IReadOnlyList<UserNotificationPreference> preferences, CancellationToken ct)
    {
        await dbContext.UserNotificationPreferences.AddRangeAsync(preferences, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => dbContext.SaveChangesAsync(ct);
}
