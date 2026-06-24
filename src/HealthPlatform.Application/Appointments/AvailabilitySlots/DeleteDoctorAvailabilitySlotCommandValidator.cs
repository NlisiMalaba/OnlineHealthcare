using FluentValidation;

namespace HealthPlatform.Application.Appointments.AvailabilitySlots;

public sealed class DeleteDoctorAvailabilitySlotCommandValidator
    : AbstractValidator<DeleteDoctorAvailabilitySlotCommand>
{
    public DeleteDoctorAvailabilitySlotCommandValidator()
    {
        RuleFor(x => x.SlotId)
            .NotEmpty();
    }
}
