namespace HealthPlatform.Application.Queue.Realtime;

public sealed record QueuePositionUpdatedRealtimeDto(
    Guid QueueEntryId,
    Guid AppointmentId,
    int QueuePosition,
    int EstimatedWaitMinutes,
    DateTime UpdatedAtUtc);

public interface IQueueRealtimeNotifier
{
    Task PublishPositionUpdatedAsync(QueuePositionUpdatedRealtimeDto update, CancellationToken ct);
}
