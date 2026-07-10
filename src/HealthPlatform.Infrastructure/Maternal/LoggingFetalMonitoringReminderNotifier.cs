using HealthPlatform.Application.Maternal.AntenatalRecords;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Maternal;

public sealed class LoggingFetalMonitoringReminderNotifier(
    ILogger<LoggingFetalMonitoringReminderNotifier> logger)
    : IFetalMonitoringReminderNotifier
{
    public Task NotifyFetalMonitoringReminderAsync(
        Guid patientUserId,
        Guid antenatalRecordId,
        int intervalDays,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Fetal monitoring reminder for record {AntenatalRecordId}, patient user {PatientUserId}, interval {IntervalDays} days.",
            antenatalRecordId,
            patientUserId,
            intervalDays);
        return Task.CompletedTask;
    }
}
