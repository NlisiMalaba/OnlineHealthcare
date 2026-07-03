using HealthPlatform.Domain.Notifications;

namespace HealthPlatform.Application.Notifications;

public interface INotificationLogRepository
{
    Task AddRangeAsync(IReadOnlyList<NotificationLog> entries, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
