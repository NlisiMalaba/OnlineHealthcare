using HealthPlatform.Application.Maternal.BirthPlans;
using MediatR;

namespace HealthPlatform.Application.Maternal.BirthPlans.UpdateBirthPlan;

public sealed record UpdateBirthPlanCommand(
    Guid AntenatalRecordId,
    BirthPlanContentDto Content) : IRequest<BirthPlanDto>;
