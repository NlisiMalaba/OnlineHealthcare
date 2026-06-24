namespace HealthPlatform.Application.Telemedicine.EndSession;

public sealed record EndTelemedicineSessionDto(
    Guid SessionId,
    Guid AppointmentId,
    string Status,
    int DurationSeconds,
    DateTime EndedAtUtc,
    string? SessionSummaryRef);
