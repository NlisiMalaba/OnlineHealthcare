namespace HealthPlatform.Application.Telemedicine.Realtime;

public sealed record TelemedicineDurationTickDto(
    Guid AppointmentId,
    int DurationSeconds,
    DateTime TickAtUtc);
