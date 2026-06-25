using FluentValidation.TestHelper;
using HealthPlatform.Application.Telemedicine.Realtime.ConnectSession;
using Xunit;

namespace HealthPlatform.Tests.Unit.Telemedicine;

public sealed class ConnectTelemedicineSessionCommandValidatorTests
{
    private readonly ConnectTelemedicineSessionCommandValidator _validator = new();

    [Fact]
    public void Should_fail_when_appointment_id_empty()
    {
        var result = _validator.TestValidate(new ConnectTelemedicineSessionCommand(Guid.Empty));
        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }
}
