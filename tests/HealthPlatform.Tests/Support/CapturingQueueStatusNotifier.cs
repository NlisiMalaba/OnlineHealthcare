using HealthPlatform.Application.Queue;

namespace HealthPlatform.Tests.Support;

public sealed record QueueStatusNotificationCall(Guid PatientUserId, Guid QueueEntryId, Guid AppointmentId);

public sealed class CapturingQueueStatusNotifier : IQueueStatusNotifier
{
    public List<QueueStatusNotificationCall> Calls { get; } = [];

    public Task NotifyMarkedAbsentAsync(Guid patientUserId, Guid queueEntryId, Guid appointmentId, CancellationToken ct)
    {
        Calls.Add(new QueueStatusNotificationCall(patientUserId, queueEntryId, appointmentId));
        return Task.CompletedTask;
    }
}
