using HealthPlatform.Domain.Maternal;

namespace HealthPlatform.Application.Maternal.AntenatalRecords;

public static class AntenatalRecordMappings
{
    public static AntenatalRecordDto ToDto(
        this AntenatalRecord record,
        IReadOnlyList<AntenatalCheckupScheduleEntry> scheduleEntries) =>
        new(
            record.Id,
            record.PatientId,
            record.EstimatedDueDate,
            record.GestationalAgeWeeks,
            record.ObstetricDoctorId,
            record.Status.ToString().ToLowerInvariant(),
            scheduleEntries.Select(entry => entry.ToDto()).ToList(),
            record.NextReminderAtUtc,
            record.CreatedAtUtc);

    public static AntenatalCheckupScheduleEntryDto ToDto(this AntenatalCheckupScheduleEntry entry) =>
        new(
            entry.Id,
            entry.GestationalAgeWeeks,
            entry.RecommendedDate,
            entry.Description);
}
