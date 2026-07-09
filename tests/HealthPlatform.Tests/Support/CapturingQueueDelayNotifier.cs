using HealthPlatform.Application.Queue;

namespace HealthPlatform.Tests.Support;

public sealed record QueueDelayNotificationCall(
    Guid PatientUserId,
    Guid QueueEntryId,
    Guid AppointmentId,
    int UpdatedEstimatedWaitMinutes,
    int DelayMinutes);

public sealed class CapturingQueueDelayNotifier : IQueueDelayNotifier
{
    public List<QueueDelayNotificationCall> Calls { get; } = [];

    public Task NotifyQueueDelayRecalculatedAsync(
        Guid patientUserId,
        Guid queueEntryId,
        Guid appointmentId,
        int updatedEstimatedWaitMinutes,
        int delayMinutes,
        CancellationToken ct)
    {
        Calls.Add(new QueueDelayNotificationCall(
            patientUserId,
            queueEntryId,
            appointmentId,
            updatedEstimatedWaitMinutes,
            delayMinutes));
        return Task.CompletedTask;
    }
}
