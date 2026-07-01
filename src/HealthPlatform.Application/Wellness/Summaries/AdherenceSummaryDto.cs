namespace HealthPlatform.Application.Wellness.Summaries;

public sealed record AdherenceSummaryDto(
    Guid PatientId,
    string Period,
    DateTime FromUtc,
    DateTime ToUtc,
    int TotalDoses,
    int TakenDoses,
    int MissedDoses,
    int LateDoses,
    double AdherenceRate,
    IReadOnlyList<MedicationScheduleAdherenceDto> Schedules);

public sealed record MedicationScheduleAdherenceDto(
    Guid ScheduleId,
    string MedicationName,
    string Status,
    int TotalDoses,
    int TakenDoses,
    int MissedDoses,
    int LateDoses,
    double AdherenceRate);
