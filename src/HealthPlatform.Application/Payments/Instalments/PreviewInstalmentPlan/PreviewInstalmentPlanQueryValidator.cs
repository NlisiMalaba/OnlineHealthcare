using FluentValidation;
using HealthPlatform.Domain.Payments.Instalments;

namespace HealthPlatform.Application.Payments.Instalments.PreviewInstalmentPlan;

public sealed class PreviewInstalmentPlanQueryValidator : AbstractValidator<PreviewInstalmentPlanQuery>
{
    public PreviewInstalmentPlanQueryValidator()
    {
        RuleFor(x => x.TotalAmountMinorUnits).GreaterThan(0);
        RuleFor(x => x.InstalmentCount).GreaterThan(0).LessThanOrEqualTo(12);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Frequency).IsInEnum();
    }
}
