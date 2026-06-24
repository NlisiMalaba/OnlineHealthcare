using FluentValidation;

namespace HealthPlatform.Application.Telemedicine.RecordingConsent;

public sealed class GrantRecordingConsentCommandValidator : AbstractValidator<GrantRecordingConsentCommand>
{
    public GrantRecordingConsentCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty();
    }
}
