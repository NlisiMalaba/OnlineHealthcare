namespace HealthPlatform.Domain.Wellness;

public sealed record HealthGoalProgress(
    Guid GoalId,
    WellnessMetricType MetricType,
    decimal TargetValue,
    decimal CurrentValue,
    string Unit,
    string? CustomLabel,
    decimal ProgressPercent,
    bool IsAchieved);

/// <summary>
/// Pure progress calculation for wellness metric entries against health goals (Req 27.2).
/// </summary>
public static class HealthGoalProgressCalculator
{
    public static HealthGoalProgress Calculate(HealthGoal goal, decimal currentValue)
    {
        ArgumentNullException.ThrowIfNull(goal);

        var progressPercent = goal.MetricType == WellnessMetricType.Weight
            ? CalculateProximityProgress(currentValue, goal.TargetValue)
            : CalculateAscendingProgress(currentValue, goal.TargetValue);

        var isAchieved = goal.MetricType == WellnessMetricType.Weight
            ? IsWithinWeightTolerance(currentValue, goal.TargetValue)
            : currentValue >= goal.TargetValue;

        return new HealthGoalProgress(
            goal.Id,
            goal.MetricType,
            goal.TargetValue,
            currentValue,
            goal.Unit,
            goal.CustomLabel,
            progressPercent,
            isAchieved);
    }

    public static IReadOnlyList<HealthGoalProgress> CalculateForActiveGoals(
        IEnumerable<HealthGoal> activeGoals,
        WellnessMetricType metricType,
        decimal currentValue) =>
        activeGoals
            .Where(goal => goal.Status == HealthGoalStatus.Active && goal.MetricType == metricType)
            .Select(goal => Calculate(goal, currentValue))
            .ToList();

    private static decimal CalculateAscendingProgress(decimal currentValue, decimal targetValue)
    {
        if (targetValue <= 0)
        {
            return 0m;
        }

        return Math.Clamp(Math.Round(currentValue / targetValue * 100m, 2, MidpointRounding.AwayFromZero), 0m, 100m);
    }

    private static decimal CalculateProximityProgress(decimal currentValue, decimal targetValue)
    {
        if (targetValue <= 0)
        {
            return 0m;
        }

        var remainingRatio = Math.Abs(currentValue - targetValue) / targetValue;
        return Math.Clamp(Math.Round((1m - remainingRatio) * 100m, 2, MidpointRounding.AwayFromZero), 0m, 100m);
    }

    private static bool IsWithinWeightTolerance(decimal currentValue, decimal targetValue) =>
        Math.Abs(currentValue - targetValue) <= Math.Max(0.1m, targetValue * 0.01m);
}
