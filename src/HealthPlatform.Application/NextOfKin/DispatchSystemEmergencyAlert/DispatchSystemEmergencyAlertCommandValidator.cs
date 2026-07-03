using FluentValidation;

namespace HealthPlatform.Application.NextOfKin.DispatchSystemEmergencyAlert;

public sealed class DispatchSystemEmergencyAlertCommandValidator : AbstractValidator<DispatchSystemEmergencyAlertCommand>
{
    public DispatchSystemEmergencyAlertCommandValidator()
    {
        RuleFor(command => command.PatientId)
            .NotEmpty();

        RuleFor(command => command.TriggerReason)
            .NotEmpty()
            .MaximumLength(500);
    }
}
