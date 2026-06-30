using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Payments.Instalments.GetInstalmentPlan;

public sealed record GetInstalmentPlanQuery(Guid PlanId) : IQuery<InstalmentPlanDto>;
