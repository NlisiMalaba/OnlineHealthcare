using HealthPlatform.Application.Notifications;
using HealthPlatform.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class NotificationLogRepository(ApplicationDbContext dbContext) : INotificationLogRepository
{
    public async Task AddRangeAsync(IReadOnlyList<NotificationLog> entries, CancellationToken ct)
    {
        await dbContext.NotificationLogs.AddRangeAsync(entries, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => dbContext.SaveChangesAsync(ct);
}
