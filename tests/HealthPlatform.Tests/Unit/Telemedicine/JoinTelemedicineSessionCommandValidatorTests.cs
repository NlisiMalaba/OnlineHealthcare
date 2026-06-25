using FluentValidation.TestHelper;
using HealthPlatform.Application.Telemedicine.JoinSession;
using HealthPlatform.Domain.Telemedicine;
using Xunit;

namespace HealthPlatform.Tests.Unit.Telemedicine;

public sealed class JoinTelemedicineSessionCommandValidatorTests
{
    private readonly JoinTelemedicineSessionCommandValidator _validator = new();

    [Fact]
    public void Should_fail_when_appointment_id_empty()
    {
        var result = _validator.TestValidate(new JoinTelemedicineSessionCommand(Guid.Empty, null));
        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }

    [Fact]
    public void Should_pass_with_valid_command()
    {
        var result = _validator.TestValidate(
            new JoinTelemedicineSessionCommand(Guid.CreateVersion7(), TelemedicineSessionMode.Audio));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
