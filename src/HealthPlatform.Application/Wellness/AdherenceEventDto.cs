namespace HealthPlatform.Application.Wellness;

public sealed record AdherenceEventDto(
    Guid Id,
    Guid ScheduleId,
    Guid PatientId,
    DateTime ScheduledAtUtc,
    DateTime? RecordedAtUtc,
    string Status);
