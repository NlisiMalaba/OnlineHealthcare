using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness.Summaries;

public static class AdherenceSummaryBuilder
{
    public static AdherenceSummaryDto Build(
        Guid patientId,
        AdherenceSummaryPeriod period,
        DateTime fromUtc,
        DateTime toUtc,
        IReadOnlyList<MedicationSchedule> schedules,
        IReadOnlyList<AdherenceEvent> events)
    {
        var eventsBySchedule = events
            .GroupBy(adherenceEvent => adherenceEvent.ScheduleId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var scheduleSummaries = new List<MedicationScheduleAdherenceDto>(schedules.Count);
        foreach (var schedule in schedules)
        {
            var scheduleEvents = eventsBySchedule.TryGetValue(schedule.Id, out var matched)
                ? matched
                : [];

            var taken = scheduleEvents.Count(e => e.Status == AdherenceEventStatus.Taken);
            var missed = scheduleEvents.Count(e => e.Status == AdherenceEventStatus.Missed);
            var late = scheduleEvents.Count(e => e.Status == AdherenceEventStatus.Late);
            var scheduledDoseCount = schedule.DoseTimes.Count(doseTime => doseTime >= fromUtc && doseTime < toUtc);
            var total = Math.Max(scheduledDoseCount, taken + missed + late);

            scheduleSummaries.Add(new MedicationScheduleAdherenceDto(
                schedule.Id,
                schedule.MedicationName,
                schedule.Status.ToString().ToLowerInvariant(),
                total,
                taken,
                missed,
                late,
                CalculateAdherenceRate(taken + late, total)));
        }

        var totalDoses = scheduleSummaries.Sum(summary => summary.TotalDoses);
        var takenDoses = scheduleSummaries.Sum(summary => summary.TakenDoses);
        var missedDoses = scheduleSummaries.Sum(summary => summary.MissedDoses);
        var lateDoses = scheduleSummaries.Sum(summary => summary.LateDoses);

        return new AdherenceSummaryDto(
            patientId,
            period.ToString().ToLowerInvariant(),
            fromUtc,
            toUtc,
            totalDoses,
            takenDoses,
            missedDoses,
            lateDoses,
            CalculateAdherenceRate(takenDoses + lateDoses, totalDoses),
            scheduleSummaries);
    }

    private static double CalculateAdherenceRate(int adherentDoses, int totalDoses) =>
        totalDoses == 0
            ? 0d
            : Math.Round((double)adherentDoses / totalDoses, 4);
}
