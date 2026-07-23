using HealthPlatform.Application.Behaviors;
using HealthPlatform.Application.Wellness.CarePlans;
using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness.CarePlans.ListCarePlans;

public sealed record ListCarePlansQuery(CarePlanStatus? Status) : IQuery<IReadOnlyList<CarePlanDto>>;
