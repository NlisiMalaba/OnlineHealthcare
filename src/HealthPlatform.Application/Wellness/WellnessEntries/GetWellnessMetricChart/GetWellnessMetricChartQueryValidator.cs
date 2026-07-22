using FluentValidation;
using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness.WellnessEntries.GetWellnessMetricChart;

public sealed class GetWellnessMetricChartQueryValidator : AbstractValidator<GetWellnessMetricChartQuery>
{
    public GetWellnessMetricChartQueryValidator()
    {
        RuleFor(query => query.MetricType).IsInEnum();

        RuleFor(query => query)
            .Must(query => !query.FromUtc.HasValue || !query.ToUtc.HasValue || query.FromUtc <= query.ToUtc)
            .WithMessage("FromUtc must be less than or equal to ToUtc.");
    }
}
