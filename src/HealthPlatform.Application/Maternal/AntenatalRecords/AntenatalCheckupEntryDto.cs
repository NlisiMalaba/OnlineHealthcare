namespace HealthPlatform.Application.Maternal.AntenatalRecords;

public sealed record AntenatalCheckupEntryDto(
    string Id,
    Guid AntenatalRecordId,
    Guid? ScheduleEntryId,
    Guid PatientId,
    Guid DoctorId,
    int GestationalAgeWeeks,
    int? FetalHeartRateBpm,
    decimal? FundalHeightCm,
    decimal? EstimatedFetalWeightGrams,
    int? BloodPressureSystolic,
    int? BloodPressureDiastolic,
    decimal? MaternalWeightKg,
    string? ClinicalNotes,
    DateTime RecordedAtUtc);
