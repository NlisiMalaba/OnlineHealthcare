using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Payments.Instalments.ListPatientInstalmentPlans;

public sealed record ListPatientInstalmentPlansQuery : IQuery<IReadOnlyList<InstalmentPlanListItemDto>>;
