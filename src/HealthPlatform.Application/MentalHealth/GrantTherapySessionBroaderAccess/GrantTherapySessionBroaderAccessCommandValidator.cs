using FluentValidation;

namespace HealthPlatform.Application.MentalHealth.GrantTherapySessionBroaderAccess;

public sealed class GrantTherapySessionBroaderAccessCommandValidator
    : AbstractValidator<GrantTherapySessionBroaderAccessCommand>
{
    public GrantTherapySessionBroaderAccessCommandValidator()
    {
        RuleFor(command => command.TherapySessionId)
            .NotEmpty();
    }
}
