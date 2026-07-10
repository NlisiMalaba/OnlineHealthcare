using HealthPlatform.Application.Queue;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Queue;

public sealed class LoggingQueueDelayNotifier(ILogger<LoggingQueueDelayNotifier> logger) : IQueueDelayNotifier
{
    public Task NotifyQueueDelayRecalculatedAsync(
        Guid patientUserId,
        Guid queueEntryId,
        Guid appointmentId,
        int updatedEstimatedWaitMinutes,
        int delayMinutes,
        CancellationToken ct)
    {
        logger.LogInformation(
            "Queue delay recalculated for queue entry {QueueEntryId}, appointment {AppointmentId}, patient {PatientUserId}. Delay {DelayMinutes} minutes, updated wait {UpdatedEstimatedWaitMinutes} minutes.",
            queueEntryId,
            appointmentId,
            patientUserId,
            delayMinutes,
            updatedEstimatedWaitMinutes);
        return Task.CompletedTask;
    }
}
