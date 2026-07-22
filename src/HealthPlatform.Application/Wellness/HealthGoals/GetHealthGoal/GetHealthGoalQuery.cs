using HealthPlatform.Application.Behaviors;
using HealthPlatform.Application.Wellness.HealthGoals;

namespace HealthPlatform.Application.Wellness.HealthGoals.GetHealthGoal;

public sealed record GetHealthGoalQuery(Guid GoalId) : IQuery<HealthGoalDto>;
