using HealthPlatform.Application.Wellness;
using HealthPlatform.Domain.Wellness;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Wellness;

public sealed class MissedDoseDetectionDispatcher(
    TimeProvider timeProvider,
    IAdherenceEventRepository adherenceEventRepository,
    IConsecutiveMissedDoseAlertService consecutiveMissedDoseAlertService,
    IMedicationScheduleCompletionService scheduleCompletionService,
    ILogger<MissedDoseDetectionDispatcher> logger) : IMissedDoseDetectionDispatcher
{
    public async Task<int> RecordMissedDosesAsync(CancellationToken ct)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var overdueDoses = await adherenceEventRepository.ListOverdueUnconfirmedDosesAsync(
            nowUtc,
            WellnessPolicies.MissedDoseDetectionBatchSize,
            ct);

        if (overdueDoses.Count == 0)
        {
            return 0;
        }

        var recorded = 0;
        foreach (var overdueDose in overdueDoses)
        {
            ct.ThrowIfCancellationRequested();

            if (!WellnessPolicies.IsMissed(overdueDose.ScheduledAtUtc, nowUtc))
            {
                continue;
            }

            var existingEvent = await adherenceEventRepository.GetByScheduleAndScheduledAtAsync(
                overdueDose.ScheduleId,
                overdueDose.ScheduledAtUtc,
                ct);
            if (existingEvent is not null)
            {
                continue;
            }

            var missedEvent = AdherenceEvent.RecordMissed(
                overdueDose.ScheduleId,
                overdueDose.PatientId,
                overdueDose.ScheduledAtUtc,
                nowUtc);

            await adherenceEventRepository.AddAsync(missedEvent, ct);
            await adherenceEventRepository.SaveChangesAsync(ct);
            await consecutiveMissedDoseAlertService.TryEmitAlertIfThresholdReachedAsync(
                overdueDose.PatientId,
                ct);
            await scheduleCompletionService.EvaluateCompletionAsync(overdueDose.ScheduleId, ct);
            recorded++;

            logger.LogInformation(
                "Recorded missed medication dose for schedule {ScheduleId} scheduled at {ScheduledAtUtc}.",
                overdueDose.ScheduleId,
                overdueDose.ScheduledAtUtc);
        }

        return recorded;
    }
}
