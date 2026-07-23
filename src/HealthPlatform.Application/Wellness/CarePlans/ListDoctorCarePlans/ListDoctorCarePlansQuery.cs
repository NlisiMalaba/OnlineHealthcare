using HealthPlatform.Application.Behaviors;
using HealthPlatform.Application.Wellness.CarePlans;
using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness.CarePlans.ListDoctorCarePlans;

public sealed record ListDoctorCarePlansQuery(
    Guid? PatientId,
    CarePlanStatus? Status) : IQuery<IReadOnlyList<CarePlanDto>>;
