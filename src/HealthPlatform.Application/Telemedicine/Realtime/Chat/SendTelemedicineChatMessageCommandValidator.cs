using FluentValidation;

namespace HealthPlatform.Application.Telemedicine.Realtime.Chat;

public sealed class SendTelemedicineChatMessageCommandValidator : AbstractValidator<SendTelemedicineChatMessageCommand>
{
    public SendTelemedicineChatMessageCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty();

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithErrorCode(TelemedicineErrorCodes.ChatMessageEmpty)
            .MaximumLength(TelemedicinePolicies.MaxChatMessageLength);
    }
}
