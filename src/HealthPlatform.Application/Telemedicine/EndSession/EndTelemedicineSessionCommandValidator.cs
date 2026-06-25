using FluentValidation;

namespace HealthPlatform.Application.Telemedicine.EndSession;

public sealed class EndTelemedicineSessionCommandValidator : AbstractValidator<EndTelemedicineSessionCommand>
{
    public EndTelemedicineSessionCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty();
    }
}
