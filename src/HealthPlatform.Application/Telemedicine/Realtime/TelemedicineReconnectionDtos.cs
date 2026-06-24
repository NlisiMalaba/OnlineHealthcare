namespace HealthPlatform.Application.Telemedicine.Realtime;

public sealed record TelemedicineReconnectionAttemptingDto(
    Guid AppointmentId,
    DateTime InterruptedAtUtc,
    DateTime ReconnectionDeadlineUtc,
    int RemainingSeconds);

public sealed record TelemedicineReconnectionSucceededDto(
    Guid AppointmentId,
    DateTime ReconnectedAtUtc);

public sealed record TelemedicineReconnectionPromptRequiredDto(
    Guid AppointmentId,
    DateTime PromptAtUtc);
