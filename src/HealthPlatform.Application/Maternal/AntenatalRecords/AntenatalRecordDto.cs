namespace HealthPlatform.Application.Maternal.AntenatalRecords;

public sealed record AntenatalCheckupScheduleEntryDto(
    Guid Id,
    int GestationalAgeWeeks,
    DateOnly RecommendedDate,
    string Description);

public sealed record AntenatalRecordDto(
    Guid Id,
    Guid PatientId,
    DateOnly EstimatedDueDate,
    int GestationalAgeWeeks,
    Guid ObstetricDoctorId,
    string Status,
    IReadOnlyList<AntenatalCheckupScheduleEntryDto> RecommendedCheckups,
    DateTime? NextReminderAtUtc,
    DateTime CreatedAtUtc);
