using FluentValidation;

namespace HealthPlatform.Application.Appointments.AvailabilitySlots;

public sealed class CreateDoctorAvailabilitySlotCommandValidator
    : AbstractValidator<CreateDoctorAvailabilitySlotCommand>
{
    public CreateDoctorAvailabilitySlotCommandValidator()
    {
        RuleFor(x => x.DayOfWeek)
            .IsInEnum();

        RuleFor(x => x.AppointmentType)
            .IsInEnum();

        RuleFor(x => x.StartTime)
            .LessThan(x => x.EndTime)
            .WithMessage("Start time must be before end time.");

        RuleFor(x => x.SlotDurationMinutes)
            .GreaterThan(0)
            .LessThanOrEqualTo(240);
    }
}
