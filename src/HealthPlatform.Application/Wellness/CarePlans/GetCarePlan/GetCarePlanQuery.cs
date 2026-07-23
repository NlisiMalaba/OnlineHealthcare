using HealthPlatform.Application.Behaviors;
using HealthPlatform.Application.Wellness.CarePlans;

namespace HealthPlatform.Application.Wellness.CarePlans.GetCarePlan;

public sealed record GetCarePlanQuery(Guid CarePlanId) : IQuery<CarePlanDto>;
