using FluentValidation;

namespace HealthPlatform.Application.PharmacyOrders.Dashboard.GetPharmacyDailySummary;

public sealed class GetPharmacyDailySummaryQueryValidator : AbstractValidator<GetPharmacyDailySummaryQuery>
{
    public GetPharmacyDailySummaryQueryValidator()
    {
        RuleFor(x => x.SummaryDate)
            .Must(date => date is null || date.Value.Year is >= 2000 and <= 9999)
            .WithMessage("Summary date year must be between 2000 and 9999.");
    }
}
