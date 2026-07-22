using FluentValidation;
using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness.WellnessEntries.ListWellnessEntries;

public sealed class ListWellnessEntriesQueryValidator : AbstractValidator<ListWellnessEntriesQuery>
{
    public ListWellnessEntriesQueryValidator()
    {
        RuleFor(query => query.MetricType)
            .IsInEnum()
            .When(query => query.MetricType.HasValue);

        RuleFor(query => query)
            .Must(query => !query.FromUtc.HasValue || !query.ToUtc.HasValue || query.FromUtc <= query.ToUtc)
            .WithMessage("FromUtc must be less than or equal to ToUtc.");
    }
}
