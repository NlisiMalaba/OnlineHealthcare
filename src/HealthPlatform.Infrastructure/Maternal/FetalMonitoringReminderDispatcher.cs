using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Maternal.AntenatalRecords;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Maternal;

public sealed class FetalMonitoringReminderDispatcher(
    TimeProvider timeProvider,
    IAntenatalRecordRepository antenatalRecordRepository,
    IPatientRepository patientRepository,
    IFetalMonitoringReminderNotifier notifier,
    ILogger<FetalMonitoringReminderDispatcher> logger) : IFetalMonitoringReminderDispatcher
{
    public async Task<int> DispatchDueRemindersAsync(CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var dueRecords = await antenatalRecordRepository.ListActiveDueForFetalMonitoringReminderAsync(now, ct);

        if (dueRecords.Count == 0)
        {
            return 0;
        }

        var dispatched = 0;
        foreach (var record in dueRecords)
        {
            ct.ThrowIfCancellationRequested();

            var patient = await patientRepository.GetByIdAsync(record.PatientId, ct);
            if (patient is null)
            {
                logger.LogWarning(
                    "Skipping fetal monitoring reminder for record {AntenatalRecordId}; patient {PatientId} was not found.",
                    record.Id,
                    record.PatientId);
                continue;
            }

            if (!record.FetalMonitoringReminderIntervalDays.HasValue)
            {
                continue;
            }

            await notifier.NotifyFetalMonitoringReminderAsync(
                patient.UserId,
                record.Id,
                record.FetalMonitoringReminderIntervalDays.Value,
                ct);

            if (!record.MarkFetalMonitoringReminderSent(now))
            {
                continue;
            }

            await antenatalRecordRepository.UpdateAsync(record, ct);
            dispatched++;

            logger.LogInformation(
                "Dispatched fetal monitoring reminder for record {AntenatalRecordId} at {IntervalDays}-day interval.",
                record.Id,
                record.FetalMonitoringReminderIntervalDays.Value);
        }

        return dispatched;
    }
}
