using FluentValidation;

namespace HealthPlatform.Application.NextOfKin.SendDoctorEmergencyAlert;

public sealed class SendDoctorEmergencyAlertCommandValidator : AbstractValidator<SendDoctorEmergencyAlertCommand>
{
    public SendDoctorEmergencyAlertCommandValidator()
    {
        RuleFor(command => command.PatientId)
            .NotEmpty();

        RuleFor(command => command.AppointmentId)
            .NotEmpty();

        RuleFor(command => command.TriggerReason)
            .NotEmpty()
            .MaximumLength(500);
    }
}
