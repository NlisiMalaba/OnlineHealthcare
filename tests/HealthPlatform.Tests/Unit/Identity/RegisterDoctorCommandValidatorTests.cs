using FluentValidation.TestHelper;
using HealthPlatform.Application.Identity.RegisterDoctor;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Xunit;

namespace HealthPlatform.Tests.Unit.Identity;

public sealed class RegisterDoctorCommandValidatorTests
{
    private readonly RegisterDoctorCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes_validation()
    {
        var result = _validator.TestValidate(DoctorRegistrationTestData.CreateValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Missing_credentials_fails_validation()
    {
        var command = DoctorRegistrationTestData.CreateValidCommand() with { Credentials = null };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Credentials);
    }

    [Fact]
    public void Empty_availability_slots_fails_validation()
    {
        var command = DoctorRegistrationTestData.CreateValidCommand() with { AvailabilitySlots = [] };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.AvailabilitySlots);
    }

    [Fact]
    public void Invalid_slot_times_fail_validation()
    {
        var command = DoctorRegistrationTestData.CreateValidCommand() with
        {
            AvailabilitySlots =
            [
                new DoctorAvailabilitySlotInput(
                    DayOfWeek.Tuesday,
                    new TimeOnly(14, 0),
                    new TimeOnly(9, 0),
                    30,
                    DoctorAppointmentType.Virtual)
            ]
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("AvailabilitySlots[0].StartTime");
    }
}
