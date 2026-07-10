using FluentValidation;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.UpdateMoodLog;

public sealed class UpdateMoodLogCommandValidator : AbstractValidator<UpdateMoodLogCommand>
{
    public UpdateMoodLogCommandValidator()
    {
        RuleFor(command => command.MoodLogId)
            .NotEmpty();

        RuleFor(command => command.Rating)
            .InclusiveBetween(MoodLogPolicies.MinRating, MoodLogPolicies.MaxRating);

        RuleFor(command => command.Notes)
            .MaximumLength(MoodLogPolicies.MaxNotesLength)
            .When(command => !string.IsNullOrWhiteSpace(command.Notes));

        RuleFor(command => command.LoggedAtUtc)
            .Must(loggedAtUtc => !loggedAtUtc.HasValue || loggedAtUtc.Value.Kind == DateTimeKind.Utc)
            .WithMessage("Logged time must be in UTC.");
    }
}
