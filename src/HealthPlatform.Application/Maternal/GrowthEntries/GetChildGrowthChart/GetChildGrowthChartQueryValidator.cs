using FluentValidation;

namespace HealthPlatform.Application.Maternal.GrowthEntries.GetChildGrowthChart;

public sealed class GetChildGrowthChartQueryValidator : AbstractValidator<GetChildGrowthChartQuery>
{
    public GetChildGrowthChartQueryValidator()
    {
        RuleFor(query => query.ChildProfileId).NotEmpty();
    }
}
