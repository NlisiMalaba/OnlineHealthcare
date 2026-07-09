namespace HealthPlatform.Application.Queue;

public interface IQueueDelayNotifier
{
    Task NotifyQueueDelayRecalculatedAsync(
        Guid patientUserId,
        Guid queueEntryId,
        Guid appointmentId,
        int updatedEstimatedWaitMinutes,
        int delayMinutes,
        CancellationToken ct);
}
