using FluentValidation;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.DeleteMoodLog;

public sealed class DeleteMoodLogCommandValidator : AbstractValidator<DeleteMoodLogCommand>
{
    public DeleteMoodLogCommandValidator()
    {
        RuleFor(command => command.MoodLogId)
            .NotEmpty();
    }
}
