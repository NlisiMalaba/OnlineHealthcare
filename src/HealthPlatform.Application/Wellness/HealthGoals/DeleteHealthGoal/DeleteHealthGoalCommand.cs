using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Wellness.HealthGoals.DeleteHealthGoal;

public sealed record DeleteHealthGoalCommand(Guid GoalId) : ICommand;
