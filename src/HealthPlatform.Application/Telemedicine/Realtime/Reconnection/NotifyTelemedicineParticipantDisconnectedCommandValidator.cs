using FluentValidation;

namespace HealthPlatform.Application.Telemedicine.Realtime.Reconnection;

public sealed class NotifyTelemedicineParticipantDisconnectedCommandValidator
    : AbstractValidator<NotifyTelemedicineParticipantDisconnectedCommand>
{
    public NotifyTelemedicineParticipantDisconnectedCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty();
    }
}
