using FluentValidation;

namespace HealthPlatform.Application.Telemedicine.RecordingConsent;

public sealed class EnableSessionRecordingCommandValidator : AbstractValidator<EnableSessionRecordingCommand>
{
    public EnableSessionRecordingCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty();
    }
}
