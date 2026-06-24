using FluentValidation.TestHelper;
using HealthPlatform.Application.Appointments.BookAppointment;
using Xunit;

namespace HealthPlatform.Tests.Unit.Appointments;

public sealed class BookAppointmentCommandValidatorTests
{
    [Fact]
    public void Valid_payload_passes_validation()
    {
        var validator = new BookAppointmentCommandValidator();
        var result = validator.TestValidate(new BookAppointmentCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateTime.UtcNow.AddDays(1)));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Non_utc_schedule_is_rejected()
    {
        var validator = new BookAppointmentCommandValidator();
        var result = validator.TestValidate(new BookAppointmentCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateTime.Now.AddDays(1)));

        result.ShouldHaveValidationErrorFor(x => x.ScheduledAtUtc);
    }
}
