using FluentValidation.TestHelper;
using HealthPlatform.Application.Appointments.AvailabilitySlots;
using Xunit;

namespace HealthPlatform.Tests.Unit.Appointments;

public sealed class GetDoctorAvailabilitySlotQueryValidatorTests
{
    [Fact]
    public void Empty_slot_id_is_rejected()
    {
        var validator = new GetDoctorAvailabilitySlotQueryValidator();
        var result = validator.TestValidate(new GetDoctorAvailabilitySlotQuery(Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.SlotId);
    }
}
