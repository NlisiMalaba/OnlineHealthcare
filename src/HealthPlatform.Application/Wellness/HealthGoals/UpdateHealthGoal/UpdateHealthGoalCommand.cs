using HealthPlatform.Application.Behaviors;
using HealthPlatform.Application.Wellness.HealthGoals;

namespace HealthPlatform.Application.Wellness.HealthGoals.UpdateHealthGoal;

public sealed record UpdateHealthGoalCommand(
    Guid GoalId,
    decimal TargetValue,
    string Unit,
    string? CustomLabel) : ICommand<HealthGoalDto>;
