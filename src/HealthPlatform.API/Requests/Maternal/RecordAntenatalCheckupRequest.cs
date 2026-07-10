namespace HealthPlatform.API.Requests.Maternal;

public sealed record RecordAntenatalCheckupRequest(
    Guid? ScheduleEntryId,
    int GestationalAgeWeeks,
    int? FetalHeartRateBpm,
    decimal? FundalHeightCm,
    decimal? EstimatedFetalWeightGrams,
    int? BloodPressureSystolic,
    int? BloodPressureDiastolic,
    decimal? MaternalWeightKg,
    string? ClinicalNotes,
    int? FetalMonitoringReminderIntervalDays);
