using HealthPlatform.Application.Queue.JoinQueue;
using Xunit;

namespace HealthPlatform.Tests.Unit.Queue;

public sealed class JoinQueueCommandValidatorTests
{
    private readonly JoinQueueCommandValidator _validator = new();

    [Fact]
    public void Validate_rejects_empty_appointment_id()
    {
        var result = _validator.Validate(new JoinQueueCommand(Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(JoinQueueCommand.AppointmentId));
    }

    [Fact]
    public void Validate_accepts_valid_command()
    {
        var result = _validator.Validate(new JoinQueueCommand(Guid.CreateVersion7()));

        Assert.True(result.IsValid);
    }
}
