using FluentValidation;

namespace HealthPlatform.Application.MentalHealth.CompleteTherapySession;

public sealed class CompleteTherapySessionCommandValidator : AbstractValidator<CompleteTherapySessionCommand>
{
    public CompleteTherapySessionCommandValidator()
    {
        RuleFor(command => command.TherapySessionId)
            .NotEmpty();

        RuleFor(command => command.SessionSummary)
            .NotEmpty()
            .MaximumLength(10_000);
    }
}
