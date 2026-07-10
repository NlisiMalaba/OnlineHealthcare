namespace HealthPlatform.Application.Maternal.AntenatalRecords;

public sealed record AntenatalCheckupEntryRecord(
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

public sealed record AntenatalCheckupEntryReference(string DocumentId);

public interface IAntenatalCheckupEntryRepository
{
    Task<AntenatalCheckupEntryReference> SaveAsync(AntenatalCheckupEntryRecord entry, CancellationToken ct);

    Task<AntenatalCheckupEntryDto?> GetByIdAsync(string entryId, CancellationToken ct);

    Task<IReadOnlyList<AntenatalCheckupEntryDto>> ListByAntenatalRecordIdAsync(
        Guid antenatalRecordId,
        CancellationToken ct);
}
