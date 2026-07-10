namespace HealthPlatform.Application.Vaccinations;

public sealed record VaccinationScheduleEntryDto(
    Guid Id,
    Guid? ChildProfileId,
    Guid? PatientId,
    string VaccineName,
    string Description,
    DateOnly RecommendedDate,
    bool IsCompleted,
    Guid? VaccinationRecordId,
    DateTime? ReminderSentAtUtc);

public sealed record VaccinationRecordDto(
    Guid Id,
    Guid? ChildProfileId,
    Guid? PatientId,
    Guid? ScheduleEntryId,
    string VaccineName,
    DateOnly AdministeredDate,
    string BatchNumber,
    string Provider,
    Guid RecordedByUserId,
    DateTime CreatedAtUtc);
