using HealthPlatform.Application.Behaviors;
using HealthPlatform.Application.Wellness.HealthGoals;
using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness.HealthGoals.ListHealthGoals;

public sealed record ListHealthGoalsQuery(HealthGoalStatus? Status = null)
    : IQuery<IReadOnlyList<HealthGoalDto>>;
