using HealthPlatform.Application.Wellness;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Wellness;

public sealed class LoggingMedicationDoseReminderNotifier(
    ILogger<LoggingMedicationDoseReminderNotifier> logger) : IMedicationDoseReminderNotifier
{
    public Task NotifyDoseReminderAsync(
        Guid patientUserId,
        Guid scheduleId,
        DateTime scheduledAtUtc,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Medication dose push reminder requested for schedule {ScheduleId}, patient user {PatientUserId}, scheduled at {ScheduledAtUtc}.",
            scheduleId,
            patientUserId,
            scheduledAtUtc);
        return Task.CompletedTask;
    }
}
