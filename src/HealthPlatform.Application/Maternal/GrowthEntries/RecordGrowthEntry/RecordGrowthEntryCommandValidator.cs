using FluentValidation;
using HealthPlatform.Application.Maternal.GrowthEntries;

namespace HealthPlatform.Application.Maternal.GrowthEntries.RecordGrowthEntry;

public sealed class RecordGrowthEntryCommandValidator : AbstractValidator<RecordGrowthEntryCommand>
{
    public RecordGrowthEntryCommandValidator()
    {
        RuleFor(command => command.ChildProfileId).NotEmpty();

        RuleFor(command => command)
            .Must(command =>
                command.HeightCm.HasValue
                || command.WeightKg.HasValue
                || !string.IsNullOrWhiteSpace(command.MilestoneNote))
            .WithMessage("At least one measurement or milestone note is required.");

        RuleFor(command => command.HeightCm)
            .InclusiveBetween(GrowthEntryPolicies.MinHeightCm, GrowthEntryPolicies.MaxHeightCm)
            .When(command => command.HeightCm.HasValue);

        RuleFor(command => command.WeightKg)
            .InclusiveBetween(GrowthEntryPolicies.MinWeightKg, GrowthEntryPolicies.MaxWeightKg)
            .When(command => command.WeightKg.HasValue);

        RuleFor(command => command.MilestoneNote)
            .MaximumLength(GrowthEntryPolicies.MaxMilestoneNoteLength)
            .When(command => !string.IsNullOrWhiteSpace(command.MilestoneNote));
    }
}
