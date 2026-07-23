using HealthPlatform.Application.Behaviors;
using HealthPlatform.Application.Wellness.CarePlans;

namespace HealthPlatform.Application.Wellness.CarePlans.AssignCarePlan;

public sealed record AssignCarePlanCommand(
    Guid PatientId,
    string Condition,
    IReadOnlyList<CarePlanTaskInput> Tasks,
    IReadOnlyList<CarePlanMonitoringTargetInput> MonitoringTargets,
    int ReviewIntervalDays) : ICommand<CarePlanDto>;
