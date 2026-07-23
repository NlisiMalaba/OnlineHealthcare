using HealthPlatform.Application.Behaviors;
using HealthPlatform.Application.Wellness.CarePlans;

namespace HealthPlatform.Application.Wellness.CarePlans.UpdateCarePlan;

public sealed record UpdateCarePlanCommand(
    Guid CarePlanId,
    string Condition,
    IReadOnlyList<CarePlanTaskInput> Tasks,
    IReadOnlyList<CarePlanMonitoringTargetInput> MonitoringTargets,
    int ReviewIntervalDays,
    DateOnly? NextReviewAt) : ICommand<CarePlanDto>;
