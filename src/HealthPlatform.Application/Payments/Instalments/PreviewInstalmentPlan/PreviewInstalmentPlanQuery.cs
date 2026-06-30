using HealthPlatform.Application.Behaviors;
using HealthPlatform.Domain.Payments.Instalments;

namespace HealthPlatform.Application.Payments.Instalments.PreviewInstalmentPlan;

public sealed record PreviewInstalmentPlanQuery(
    long TotalAmountMinorUnits,
    InstalmentFrequency Frequency,
    int InstalmentCount,
    string Currency,
    DateOnly FirstDueDate) : IQuery<InstalmentPlanPreviewDto>;
