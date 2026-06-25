using FluentValidation.TestHelper;
using HealthPlatform.Application.Telemedicine.Realtime.Reconnection;
using Xunit;

namespace HealthPlatform.Tests.Unit.Telemedicine;

public sealed class NotifyTelemedicineParticipantDisconnectedCommandValidatorTests
{
    private readonly NotifyTelemedicineParticipantDisconnectedCommandValidator _validator = new();

    [Fact]
    public void Should_fail_when_appointment_id_empty()
    {
        var result = _validator.TestValidate(
            new NotifyTelemedicineParticipantDisconnectedCommand(Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }
}
