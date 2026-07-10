using FluentValidation;

namespace HealthPlatform.Application.Maternal.GrowthEntries.ListGrowthEntries;

public sealed class ListGrowthEntriesQueryValidator : AbstractValidator<ListGrowthEntriesQuery>
{
    public ListGrowthEntriesQueryValidator()
    {
        RuleFor(query => query.ChildProfileId).NotEmpty();
    }
}
