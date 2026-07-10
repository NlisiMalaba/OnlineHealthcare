using HealthPlatform.Domain.Maternal;

namespace HealthPlatform.Application.Maternal.AntenatalRecords;

public interface IAntenatalRecordRepository
{
    Task AddAsync(AntenatalRecord record, CancellationToken ct);

    Task<AntenatalRecord?> GetByIdAsync(Guid antenatalRecordId, CancellationToken ct);

    Task<AntenatalRecord?> GetActiveByPatientIdAsync(Guid patientId, CancellationToken ct);

    Task UpdateAsync(AntenatalRecord record, CancellationToken ct);

    Task AddScheduleEntriesAsync(
        IReadOnlyCollection<AntenatalCheckupScheduleEntry> entries,
        CancellationToken ct);

    Task<IReadOnlyList<AntenatalCheckupScheduleEntry>> ListScheduleEntriesByRecordIdAsync(
        Guid antenatalRecordId,
        CancellationToken ct);

    Task<bool> HasScheduleEntriesAsync(Guid antenatalRecordId, CancellationToken ct);

    Task<IReadOnlyList<AntenatalRecord>> ListActiveDueForReminderAsync(DateTime asOfUtc, CancellationToken ct);
}
