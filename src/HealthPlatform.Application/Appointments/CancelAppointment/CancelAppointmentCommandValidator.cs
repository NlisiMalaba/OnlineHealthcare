using FluentValidation;

namespace HealthPlatform.Application.Appointments.CancelAppointment;

public sealed class CancelAppointmentCommandValidator : AbstractValidator<CancelAppointmentCommand>
{
    public CancelAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty();

        When(x => x.Reason is not null, () =>
        {
            RuleFor(x => x.Reason)
                .MaximumLength(500);
        });
    }
}
