using HealthPlatform.Application.Maternal.BirthPlans;
using MediatR;

namespace HealthPlatform.Application.Maternal.BirthPlans.CreateBirthPlan;

public sealed record CreateBirthPlanCommand(
    Guid AntenatalRecordId,
    BirthPlanContentDto Content) : IRequest<BirthPlanDto>;
