using HealthPlatform.Domain.Vaccinations;

namespace HealthPlatform.Application.Vaccinations;

public static class VaccinationMappings
{
    public static VaccinationScheduleEntryDto ToDto(this VaccinationScheduleEntry entry) =>
        new(
            entry.Id,
            entry.ChildProfileId,
            entry.PatientId,
            entry.VaccineName,
            entry.Description,
            entry.RecommendedDate,
            entry.CompletedAtUtc.HasValue,
            entry.VaccinationRecordId,
            entry.ReminderSentAtUtc);

    public static VaccinationRecordDto ToDto(this VaccinationRecord record) =>
        new(
            record.Id,
            record.ChildProfileId,
            record.PatientId,
            record.ScheduleEntryId,
            record.VaccineName,
            record.AdministeredDate,
            record.BatchNumber,
            record.Provider,
            record.RecordedByUserId,
            record.CreatedAtUtc);
}
