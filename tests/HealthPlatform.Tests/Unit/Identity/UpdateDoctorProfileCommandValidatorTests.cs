using FluentValidation.TestHelper;
using HealthPlatform.Application.Identity.RegisterDoctor;
using HealthPlatform.Application.Identity.UpdateDoctorProfile;
using HealthPlatform.Domain.Identity;
using Xunit;

namespace HealthPlatform.Tests.Unit.Identity;

public sealed class UpdateDoctorProfileCommandValidatorTests
{
    private readonly UpdateDoctorProfileCommandValidator _validator = new();

    [Fact]
    public void Empty_update_is_rejected()
    {
        var result = _validator.TestValidate(new UpdateDoctorProfileCommand(
            null,
            null,
            null,
            null,
            null,
            null));

        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Valid_fee_update_passes()
    {
        var result = _validator.TestValidate(new UpdateDoctorProfileCommand(
            30m,
            45m,
            null,
            null,
            null,
            null));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Negative_fee_is_rejected()
    {
        var result = _validator.TestValidate(new UpdateDoctorProfileCommand(
            -1m,
            null,
            null,
            null,
            null,
            null));

        result.ShouldHaveValidationErrorFor(x => x.VirtualFee);
    }

    [Fact]
    public void Empty_availability_list_is_rejected()
    {
        var result = _validator.TestValidate(new UpdateDoctorProfileCommand(
            null,
            null,
            null,
            [],
            null,
            null));

        result.ShouldHaveValidationErrorFor(x => x.AvailabilitySlots);
    }

    [Fact]
    public void Invalid_availability_slot_is_rejected()
    {
        var result = _validator.TestValidate(new UpdateDoctorProfileCommand(
            null,
            null,
            null,
            [
                new DoctorAvailabilitySlotInput(
                    DayOfWeek.Monday,
                    new TimeOnly(12, 0),
                    new TimeOnly(9, 0),
                    30,
                    DoctorAppointmentType.Virtual)
            ],
            null,
            null));

        result.ShouldHaveValidationErrorFor("AvailabilitySlots[0].StartTime");
    }
}
