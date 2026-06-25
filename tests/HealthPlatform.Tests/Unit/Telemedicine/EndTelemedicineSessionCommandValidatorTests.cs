using FluentValidation.TestHelper;
using HealthPlatform.Application.Telemedicine.EndSession;
using Xunit;

namespace HealthPlatform.Tests.Unit.Telemedicine;

public sealed class EndTelemedicineSessionCommandValidatorTests
{
    private readonly EndTelemedicineSessionCommandValidator _validator = new();

    [Fact]
    public void Should_fail_when_appointment_id_empty()
    {
        var result = _validator.TestValidate(new EndTelemedicineSessionCommand(Guid.Empty));
        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }
}
