using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Telemedicine.Realtime.Chat;

public sealed class SendTelemedicineChatMessageCommandHandler(
    TimeProvider timeProvider,
    ITelemedicineSessionParticipantService participantService,
    ITelemedicineRealtimeNotifier realtimeNotifier,
    ILogger<SendTelemedicineChatMessageCommandHandler> logger)
    : IRequestHandler<SendTelemedicineChatMessageCommand, TelemedicineChatMessageDto>
{
    public async Task<TelemedicineChatMessageDto> Handle(
        SendTelemedicineChatMessageCommand request,
        CancellationToken ct)
    {
        var participant = await participantService.ResolveParticipantAsync(
            request.AppointmentId,
            requireActiveSession: true,
            ct);

        var senderRole = participant.Role switch
        {
            TelemedicineSessionParticipantRole.Patient => ApplicationRoles.Patient,
            TelemedicineSessionParticipantRole.Doctor => ApplicationRoles.Doctor,
            _ => throw new InvalidOperationException("Unsupported participant role.")
        };

        var message = new TelemedicineChatMessageDto(
            request.AppointmentId,
            Guid.CreateVersion7(),
            senderRole,
            timeProvider.GetUtcNow().UtcDateTime,
            request.Message.Trim());

        await realtimeNotifier.PublishChatMessageAsync(message, ct);

        logger.LogInformation(
            "Published telemedicine chat message {MessageId} for appointment {AppointmentId}.",
            message.MessageId,
            request.AppointmentId);

        return message;
    }
}
