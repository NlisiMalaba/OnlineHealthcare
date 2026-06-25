using FluentValidation.TestHelper;
using HealthPlatform.Application.Telemedicine.Realtime.Chat;
using Xunit;

namespace HealthPlatform.Tests.Unit.Telemedicine;

public sealed class SendTelemedicineChatMessageCommandValidatorTests
{
    private readonly SendTelemedicineChatMessageCommandValidator _validator = new();

    [Fact]
    public void Should_fail_when_message_empty()
    {
        var result = _validator.TestValidate(
            new SendTelemedicineChatMessageCommand(Guid.CreateVersion7(), " "));

        result.ShouldHaveValidationErrorFor(x => x.Message);
    }
}
