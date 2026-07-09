using HealthPlatform.Application.Queue;

namespace HealthPlatform.Tests.Support;

public sealed record QueuePositionNotificationCall(
    Guid PatientUserId,
    Guid QueueEntryId,
    Guid AppointmentId,
    int EstimatedWaitMinutes);

public sealed class CapturingQueuePositionNotifier : IQueuePositionNotifier
{
    public List<QueuePositionNotificationCall> Calls { get; } = [];

    public Task NotifySecondPositionReachedAsync(
        Guid patientUserId,
        Guid queueEntryId,
        Guid appointmentId,
        int estimatedWaitMinutes,
        CancellationToken ct)
    {
        Calls.Add(new QueuePositionNotificationCall(
            patientUserId,
            queueEntryId,
            appointmentId,
            estimatedWaitMinutes));
        return Task.CompletedTask;
    }
}
