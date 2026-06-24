using FluentValidation;

namespace HealthPlatform.Application.Appointments.BookAppointment;

public sealed class BookAppointmentCommandValidator : AbstractValidator<BookAppointmentCommand>
{
    public BookAppointmentCommandValidator()
    {
        RuleFor(x => x.DoctorId)
            .NotEmpty();

        RuleFor(x => x.SlotId)
            .NotEmpty();

        RuleFor(x => x.ScheduledAtUtc)
            .Must(v => v != default)
            .WithMessage("Scheduled time is required.");

        RuleFor(x => x.ScheduledAtUtc)
            .Must(v => v.Kind == DateTimeKind.Utc)
            .WithMessage("Scheduled time must be in UTC.");
    }
}
