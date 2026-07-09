using FluentValidation;

namespace HealthPlatform.Application.Queue.JoinQueue;

public sealed class JoinQueueCommandValidator : AbstractValidator<JoinQueueCommand>
{
    public JoinQueueCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty();
    }
}
