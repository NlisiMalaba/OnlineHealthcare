namespace HealthPlatform.Application.Telemedicine.Realtime;

public sealed record TelemedicineChatMessageDto(
    Guid AppointmentId,
    Guid MessageId,
    string SenderRole,
    DateTime SentAtUtc,
    string Message);
