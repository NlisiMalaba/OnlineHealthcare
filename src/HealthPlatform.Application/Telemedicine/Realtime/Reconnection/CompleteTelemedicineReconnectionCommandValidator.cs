using FluentValidation;

namespace HealthPlatform.Application.Telemedicine.Realtime.Reconnection;

public sealed class CompleteTelemedicineReconnectionCommandValidator
    : AbstractValidator<CompleteTelemedicineReconnectionCommand>
{
    public CompleteTelemedicineReconnectionCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty();
    }
}
