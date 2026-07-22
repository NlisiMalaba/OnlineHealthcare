using FluentValidation;
using HealthPlatform.Domain.Wellness;

namespace HealthPlatform.Application.Wellness.WellnessEntries.RecordWellnessEntry;

public sealed class RecordWellnessEntryCommandValidator : AbstractValidator<RecordWellnessEntryCommand>
{
    public RecordWellnessEntryCommandValidator()
    {
        RuleFor(command => command.MetricType).IsInEnum();

        RuleFor(command => command.Value)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(WellnessPolicies.MaxEntryValue);

        RuleFor(command => command.GoalId)
            .NotEqual(Guid.Empty)
            .When(command => command.GoalId.HasValue);
    }
}
