using FluentValidation.TestHelper;
using HealthPlatform.Application.Appointments.AvailabilitySlots;
using HealthPlatform.Domain.Identity;
using Xunit;

namespace HealthPlatform.Tests.Unit.Appointments;

public sealed class DoctorAvailabilitySlotCommandValidatorTests
{
    [Fact]
    public void Create_validator_accepts_valid_payload()
    {
        var validator = new CreateDoctorAvailabilitySlotCommandValidator();
        var result = validator.TestValidate(new CreateDoctorAvailabilitySlotCommand(
            DayOfWeek.Wednesday,
            new TimeOnly(9, 0),
            new TimeOnly(12, 0),
            30,
            DoctorAppointmentType.Virtual));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Create_validator_rejects_invalid_time_range()
    {
        var validator = new CreateDoctorAvailabilitySlotCommandValidator();
        var result = validator.TestValidate(new CreateDoctorAvailabilitySlotCommand(
            DayOfWeek.Wednesday,
            new TimeOnly(12, 0),
            new TimeOnly(9, 0),
            30,
            DoctorAppointmentType.Virtual));

        result.ShouldHaveValidationErrorFor(x => x.StartTime);
    }

    [Fact]
    public void Update_validator_rejects_empty_slot_id()
    {
        var validator = new UpdateDoctorAvailabilitySlotCommandValidator();
        var result = validator.TestValidate(new UpdateDoctorAvailabilitySlotCommand(
            Guid.Empty,
            DayOfWeek.Friday,
            new TimeOnly(10, 0),
            new TimeOnly(13, 0),
            45,
            DoctorAppointmentType.Physical));

        result.ShouldHaveValidationErrorFor(x => x.SlotId);
    }

    [Fact]
    public void Delete_validator_rejects_empty_slot_id()
    {
        var validator = new DeleteDoctorAvailabilitySlotCommandValidator();
        var result = validator.TestValidate(new DeleteDoctorAvailabilitySlotCommand(Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.SlotId);
    }
}
