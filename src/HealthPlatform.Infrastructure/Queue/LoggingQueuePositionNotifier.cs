using HealthPlatform.Application.Queue;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Queue;

public sealed class LoggingQueuePositionNotifier(ILogger<LoggingQueuePositionNotifier> logger)
    : IQueuePositionNotifier
{
    public Task NotifySecondPositionReachedAsync(
        Guid patientUserId,
        Guid queueEntryId,
        Guid appointmentId,
        int estimatedWaitMinutes,
        CancellationToken ct)
    {
        logger.LogInformation(
            "Queue entry {QueueEntryId} for appointment {AppointmentId} reached second position for patient {PatientUserId} with estimated wait {EstimatedWaitMinutes} minutes.",
            queueEntryId,
            appointmentId,
            patientUserId,
            estimatedWaitMinutes);
        return Task.CompletedTask;
    }
}
