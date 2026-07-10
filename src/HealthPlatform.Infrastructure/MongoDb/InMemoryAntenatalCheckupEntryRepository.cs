using HealthPlatform.Application.Maternal.AntenatalRecords;
using MongoDB.Bson;

namespace HealthPlatform.Infrastructure.MongoDb;

public sealed class InMemoryAntenatalCheckupEntryRepository : IAntenatalCheckupEntryRepository
{
    private readonly Dictionary<string, AntenatalCheckupEntryDto> _entries = new(StringComparer.Ordinal);

    public Task<AntenatalCheckupEntryReference> SaveAsync(AntenatalCheckupEntryRecord entry, CancellationToken ct)
    {
        var documentId = ObjectId.GenerateNewId().ToString();
        var dto = new AntenatalCheckupEntryDto(
            documentId,
            entry.AntenatalRecordId,
            entry.ScheduleEntryId,
            entry.PatientId,
            entry.DoctorId,
            entry.GestationalAgeWeeks,
            entry.FetalHeartRateBpm,
            entry.FundalHeightCm,
            entry.EstimatedFetalWeightGrams,
            entry.BloodPressureSystolic,
            entry.BloodPressureDiastolic,
            entry.MaternalWeightKg,
            entry.ClinicalNotes,
            entry.RecordedAtUtc);

        _entries[documentId] = dto;
        return Task.FromResult(new AntenatalCheckupEntryReference(documentId));
    }

    public Task<AntenatalCheckupEntryDto?> GetByIdAsync(string entryId, CancellationToken ct) =>
        Task.FromResult(_entries.GetValueOrDefault(entryId));

    public Task<IReadOnlyList<AntenatalCheckupEntryDto>> ListByAntenatalRecordIdAsync(
        Guid antenatalRecordId,
        CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<AntenatalCheckupEntryDto>>(
            _entries.Values
                .Where(entry => entry.AntenatalRecordId == antenatalRecordId)
                .OrderByDescending(entry => entry.RecordedAtUtc)
                .ToList());
}
