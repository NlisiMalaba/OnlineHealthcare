using FluentValidation.TestHelper;
using HealthPlatform.Application.Appointments.CancelAppointment;
using Xunit;

namespace HealthPlatform.Tests.Unit.Appointments;

public sealed class CancelAppointmentCommandValidatorTests
{
    [Fact]
    public void Valid_command_passes_validation()
    {
        var validator = new CancelAppointmentCommandValidator();
        var result = validator.TestValidate(new CancelAppointmentCommand(
            Guid.CreateVersion7(),
            "Schedule conflict"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_appointment_id_is_rejected()
    {
        var validator = new CancelAppointmentCommandValidator();
        var result = validator.TestValidate(new CancelAppointmentCommand(Guid.Empty, null));

        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }
}
