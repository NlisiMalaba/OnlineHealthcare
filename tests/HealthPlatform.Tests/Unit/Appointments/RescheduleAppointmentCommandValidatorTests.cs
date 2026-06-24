using FluentValidation.TestHelper;
using HealthPlatform.Application.Appointments.RescheduleAppointment;
using Xunit;

namespace HealthPlatform.Tests.Unit.Appointments;

public sealed class RescheduleAppointmentCommandValidatorTests
{
    [Fact]
    public void Valid_command_passes_validation()
    {
        var validator = new RescheduleAppointmentCommandValidator();
        var result = validator.TestValidate(new RescheduleAppointmentCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateTime.UtcNow.AddDays(2)));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_appointment_id_is_rejected()
    {
        var validator = new RescheduleAppointmentCommandValidator();
        var result = validator.TestValidate(new RescheduleAppointmentCommand(
            Guid.Empty,
            Guid.CreateVersion7(),
            DateTime.UtcNow.AddDays(2)));

        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }

    [Fact]
    public void Empty_slot_id_is_rejected()
    {
        var validator = new RescheduleAppointmentCommandValidator();
        var result = validator.TestValidate(new RescheduleAppointmentCommand(
            Guid.CreateVersion7(),
            Guid.Empty,
            DateTime.UtcNow.AddDays(2)));

        result.ShouldHaveValidationErrorFor(x => x.NewSlotId);
    }
}
