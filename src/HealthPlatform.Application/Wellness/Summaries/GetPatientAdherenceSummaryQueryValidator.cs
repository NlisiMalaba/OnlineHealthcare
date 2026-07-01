using FluentValidation;

namespace HealthPlatform.Application.Wellness.Summaries;

public sealed class GetPatientAdherenceSummaryQueryValidator : AbstractValidator<GetPatientAdherenceSummaryQuery>
{
    public GetPatientAdherenceSummaryQueryValidator()
    {
        RuleFor(query => query.Period).IsInEnum();
    }
}
