using HealthPlatform.Application.Maternal.BirthPlans;
using MediatR;

namespace HealthPlatform.Application.Maternal.BirthPlans.GetBirthPlan;

public sealed record GetBirthPlanQuery(Guid AntenatalRecordId) : IRequest<BirthPlanDto>;
