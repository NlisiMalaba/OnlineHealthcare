using FluentValidation;

namespace HealthPlatform.Application.Telemedicine.Realtime.ConnectSession;

public sealed class ConnectTelemedicineSessionCommandValidator : AbstractValidator<ConnectTelemedicineSessionCommand>
{
    public ConnectTelemedicineSessionCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty();
    }
}
