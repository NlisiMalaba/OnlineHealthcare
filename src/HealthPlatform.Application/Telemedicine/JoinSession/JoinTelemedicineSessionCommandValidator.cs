using FluentValidation;
using HealthPlatform.Application.Telemedicine.JoinSession;

namespace HealthPlatform.Application.Telemedicine.JoinSession;

public sealed class JoinTelemedicineSessionCommandValidator : AbstractValidator<JoinTelemedicineSessionCommand>
{
    public JoinTelemedicineSessionCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty();

        RuleFor(x => x.Mode)
            .IsInEnum()
            .When(x => x.Mode.HasValue);
    }
}
