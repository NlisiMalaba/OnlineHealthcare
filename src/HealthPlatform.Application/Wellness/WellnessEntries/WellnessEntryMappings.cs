using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness.WellnessEntries;

public static class WellnessEntryMappings
{
    public static HealthGoalProgressDto ToDto(this HealthGoalProgress progress) =>
        new(
            progress.GoalId,
            progress.MetricType,
            progress.TargetValue,
            progress.CurrentValue,
            progress.Unit,
            progress.CustomLabel,
            progress.ProgressPercent,
            progress.IsAchieved);

    public static WellnessEntryDto ToDto(
        this WellnessEntry entry,
        IReadOnlyList<HealthGoalProgress> goalProgress) =>
        new(
            entry.Id,
            entry.PatientId,
            entry.GoalId,
            entry.MetricType,
            entry.Value,
            entry.RecordedAtUtc,
            goalProgress.Select(progress => progress.ToDto()).ToList());

    public static WellnessEntryDto ToDto(this WellnessEntry entry) =>
        entry.ToDto([]);
}
