namespace HealthPlatform.Application.Queue;

public interface IQueuePositionNotifier
{
    Task NotifySecondPositionReachedAsync(
        Guid patientUserId,
        Guid queueEntryId,
        Guid appointmentId,
        int estimatedWaitMinutes,
        CancellationToken ct);
}
