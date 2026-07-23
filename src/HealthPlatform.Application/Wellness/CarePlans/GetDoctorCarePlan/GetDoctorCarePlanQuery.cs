using HealthPlatform.Application.Behaviors;
using HealthPlatform.Application.Wellness.CarePlans;

namespace HealthPlatform.Application.Wellness.CarePlans.GetDoctorCarePlan;

public sealed record GetDoctorCarePlanQuery(Guid CarePlanId) : IQuery<CarePlanDto>;
