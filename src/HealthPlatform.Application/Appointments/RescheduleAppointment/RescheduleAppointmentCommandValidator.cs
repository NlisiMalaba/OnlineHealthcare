using FluentValidation;

namespace HealthPlatform.Application.Appointments.RescheduleAppointment;

public sealed class RescheduleAppointmentCommandValidator : AbstractValidator<RescheduleAppointmentCommand>
{
    public RescheduleAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty();

        RuleFor(x => x.NewSlotId)
            .NotEmpty();

        RuleFor(x => x.NewScheduledAtUtc)
            .Must(v => v != default)
            .WithMessage("New scheduled time is required.");

        RuleFor(x => x.NewScheduledAtUtc)
            .Must(v => v.Kind == DateTimeKind.Utc)
            .WithMessage("New scheduled time must be in UTC.");
    }
}
