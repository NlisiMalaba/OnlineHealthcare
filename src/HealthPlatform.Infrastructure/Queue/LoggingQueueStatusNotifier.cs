using HealthPlatform.Application.Queue;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Queue;

public sealed class LoggingQueueStatusNotifier(ILogger<LoggingQueueStatusNotifier> logger) : IQueueStatusNotifier
{
    public Task NotifyMarkedAbsentAsync(
        Guid patientUserId,
        Guid queueEntryId,
        Guid appointmentId,
        CancellationToken ct)
    {
        logger.LogInformation(
            "Queue entry {QueueEntryId} for appointment {AppointmentId} marked absent for patient {PatientUserId}.",
            queueEntryId,
            appointmentId,
            patientUserId);
        return Task.CompletedTask;
    }
}
