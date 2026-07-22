using HealthPlatform.Application.Behaviors;
using HealthPlatform.Application.Wellness.HealthGoals;
using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness.HealthGoals.CreateHealthGoal;

public sealed record CreateHealthGoalCommand(
    WellnessMetricType MetricType,
    decimal TargetValue,
    string? Unit,
    string? CustomLabel) : ICommand<HealthGoalDto>;
