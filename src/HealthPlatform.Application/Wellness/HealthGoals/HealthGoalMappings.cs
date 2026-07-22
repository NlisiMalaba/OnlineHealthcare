using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness.HealthGoals;

public static class HealthGoalMappings
{
    public static HealthGoalDto ToDto(this HealthGoal goal) =>
        new(
            goal.Id,
            goal.PatientId,
            goal.MetricType,
            goal.TargetValue,
            goal.Unit,
            goal.CustomLabel,
            goal.Status,
            goal.CreatedAtUtc);

    public static string ResolveDefaultUnit(WellnessMetricType metricType) =>
        metricType switch
        {
            WellnessMetricType.Steps => WellnessPolicies.DefaultStepsUnit,
            WellnessMetricType.Weight => WellnessPolicies.DefaultWeightUnit,
            WellnessMetricType.SleepHours => WellnessPolicies.DefaultSleepHoursUnit,
            WellnessMetricType.WaterMl => WellnessPolicies.DefaultWaterMlUnit,
            WellnessMetricType.Custom => string.Empty,
            _ => string.Empty
        };
}
