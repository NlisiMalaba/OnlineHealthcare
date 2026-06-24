using FluentValidation;

namespace HealthPlatform.Application.Appointments.AvailabilitySlots;

public sealed class GetDoctorAvailabilitySlotQueryValidator : AbstractValidator<GetDoctorAvailabilitySlotQuery>
{
    public GetDoctorAvailabilitySlotQueryValidator()
    {
        RuleFor(x => x.SlotId)
            .NotEmpty();
    }
}
