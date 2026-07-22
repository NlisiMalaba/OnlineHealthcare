using HealthPlatform.Domain.Wellness;
using Xunit;

namespace HealthPlatform.Tests.Unit.Wellness;

public sealed class HealthGoalProgressCalculatorTests
{
    [Theory]
    [InlineData(5000, 10000, 50, false)]
    [InlineData(10000, 10000, 100, true)]
    [InlineData(15000, 10000, 100, true)]
    [InlineData(0, 10000, 0, false)]
    public void Calculate_ascending_metrics_reports_clamped_percent_of_target(
        decimal current,
        decimal target,
        decimal expectedPercent,
        bool expectedAchieved)
    {
        var goal = CreateGoal(WellnessMetricType.Steps, target, "steps");

        var progress = HealthGoalProgressCalculator.Calculate(goal, current);

        Assert.Equal(expectedPercent, progress.ProgressPercent);
        Assert.Equal(expectedAchieved, progress.IsAchieved);
        Assert.Equal(current, progress.CurrentValue);
        Assert.Equal(target, progress.TargetValue);
    }

    [Fact]
    public void Calculate_weight_uses_proximity_to_target()
    {
        var goal = CreateGoal(WellnessMetricType.Weight, 70m, "kg");

        var onTarget = HealthGoalProgressCalculator.Calculate(goal, 70m);
        var nearTarget = HealthGoalProgressCalculator.Calculate(goal, 73.5m);
        var farFromTarget = HealthGoalProgressCalculator.Calculate(goal, 100m);

        Assert.Equal(100m, onTarget.ProgressPercent);
        Assert.True(onTarget.IsAchieved);
        Assert.Equal(95m, nearTarget.ProgressPercent);
        Assert.False(nearTarget.IsAchieved);
        Assert.True(farFromTarget.ProgressPercent < nearTarget.ProgressPercent);
    }

    [Fact]
    public void CalculateForActiveGoals_only_includes_matching_active_goals()
    {
        var patientId = Guid.CreateVersion7();
        var now = DateTime.UtcNow;
        var activeSteps = HealthGoal.Create(patientId, WellnessMetricType.Steps, 10000m, "steps", null, now);
        var archivedSteps = HealthGoal.Create(patientId, WellnessMetricType.Steps, 8000m, "steps", null, now);
        archivedSteps.Archive();
        var activeWeight = HealthGoal.Create(patientId, WellnessMetricType.Weight, 70m, "kg", null, now);

        var progress = HealthGoalProgressCalculator.CalculateForActiveGoals(
            [activeSteps, archivedSteps, activeWeight],
            WellnessMetricType.Steps,
            5000m);

        Assert.Single(progress);
        Assert.Equal(activeSteps.Id, progress[0].GoalId);
        Assert.Equal(50m, progress[0].ProgressPercent);
    }

    private static HealthGoal CreateGoal(WellnessMetricType metricType, decimal target, string unit) =>
        HealthGoal.Create(Guid.CreateVersion7(), metricType, target, unit, null, DateTime.UtcNow);
}
