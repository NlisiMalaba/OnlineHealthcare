using FluentValidation.TestHelper;
using HealthPlatform.Application.Telemedicine.RecordingConsent;
using Xunit;

namespace HealthPlatform.Tests.Unit.Telemedicine;

public sealed class EnableSessionRecordingCommandValidatorTests
{
    private readonly EnableSessionRecordingCommandValidator _validator = new();

    [Fact]
    public void Should_fail_when_appointment_id_empty()
    {
        var result = _validator.TestValidate(new EnableSessionRecordingCommand(Guid.Empty));
        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }
}
