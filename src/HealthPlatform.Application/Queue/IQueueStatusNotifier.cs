namespace HealthPlatform.Application.Queue;

public interface IQueueStatusNotifier
{
    Task NotifyMarkedAbsentAsync(
        Guid patientUserId,
        Guid queueEntryId,
        Guid appointmentId,
        CancellationToken ct);
}
