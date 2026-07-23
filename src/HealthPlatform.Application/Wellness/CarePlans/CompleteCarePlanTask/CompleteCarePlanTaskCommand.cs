using HealthPlatform.Application.Behaviors;
using HealthPlatform.Application.Wellness.CarePlans;

namespace HealthPlatform.Application.Wellness.CarePlans.CompleteCarePlanTask;

public sealed record CompleteCarePlanTaskCommand(
    Guid CarePlanId,
    Guid TaskId) : ICommand<CarePlanDto>;
