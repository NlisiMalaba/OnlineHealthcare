namespace HealthPlatform.Application.Notifications;

public interface ICriticalNotificationSmsFallbackService
{
    Task ScheduleAsync(
        NotificationDispatchRequest request,
        ResolvedNotificationRecipient recipient,
        CancellationToken ct);

    Task<bool> ProcessAsync(Guid fallbackId, CancellationToken ct);

    Task<int> ProcessDueAsync(CancellationToken ct);
}
