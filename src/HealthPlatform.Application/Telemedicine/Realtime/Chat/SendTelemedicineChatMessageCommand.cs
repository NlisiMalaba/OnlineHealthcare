using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Telemedicine.Realtime.Chat;

public sealed record SendTelemedicineChatMessageCommand(
    Guid AppointmentId,
    string Message) : ICommand<TelemedicineChatMessageDto>;
