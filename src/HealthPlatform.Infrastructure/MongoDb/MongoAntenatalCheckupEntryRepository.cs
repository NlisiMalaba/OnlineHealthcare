using HealthPlatform.Application.Maternal.AntenatalRecords;
using HealthPlatform.Infrastructure.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HealthPlatform.Infrastructure.MongoDb;

public static class AntenatalCheckupEntryDocumentMapper
{
    public static AntenatalCheckupEntryDto ToDto(AntenatalCheckupEntryDocument document) =>
        new(
            document.Id.ToString(),
            document.AntenatalRecordId,
            document.ScheduleEntryId,
            document.PatientId,
            document.DoctorId,
            document.GestationalAgeWeeks,
            document.FetalHeartRateBpm,
            document.FundalHeightCm,
            document.EstimatedFetalWeightGrams,
            document.BloodPressureSystolic,
            document.BloodPressureDiastolic,
            document.MaternalWeightKg,
            document.ClinicalNotes,
            document.RecordedAtUtc);

    public static AntenatalCheckupEntryDocument ToDocument(AntenatalCheckupEntryRecord entry) =>
        new()
        {
            AntenatalRecordId = entry.AntenatalRecordId,
            ScheduleEntryId = entry.ScheduleEntryId,
            PatientId = entry.PatientId,
            DoctorId = entry.DoctorId,
            GestationalAgeWeeks = entry.GestationalAgeWeeks,
            FetalHeartRateBpm = entry.FetalHeartRateBpm,
            FundalHeightCm = entry.FundalHeightCm,
            EstimatedFetalWeightGrams = entry.EstimatedFetalWeightGrams,
            BloodPressureSystolic = entry.BloodPressureSystolic,
            BloodPressureDiastolic = entry.BloodPressureDiastolic,
            MaternalWeightKg = entry.MaternalWeightKg,
            ClinicalNotes = entry.ClinicalNotes,
            RecordedAtUtc = entry.RecordedAtUtc
        };
}

public sealed class MongoAntenatalCheckupEntryRepository(IMongoDatabase database)
    : IAntenatalCheckupEntryRepository
{
    private const string CollectionName = "antenatal_checkup_entries";

    public async Task<AntenatalCheckupEntryReference> SaveAsync(
        AntenatalCheckupEntryRecord entry,
        CancellationToken ct)
    {
        var document = AntenatalCheckupEntryDocumentMapper.ToDocument(entry);
        await database
            .GetCollection<AntenatalCheckupEntryDocument>(CollectionName)
            .InsertOneAsync(document, cancellationToken: ct);

        return new AntenatalCheckupEntryReference(document.Id.ToString());
    }

    public async Task<AntenatalCheckupEntryDto?> GetByIdAsync(string entryId, CancellationToken ct)
    {
        if (!ObjectId.TryParse(entryId, out var objectId))
        {
            return null;
        }

        var document = await database
            .GetCollection<AntenatalCheckupEntryDocument>(CollectionName)
            .Find(entry => entry.Id == objectId)
            .FirstOrDefaultAsync(ct);

        return document is null ? null : AntenatalCheckupEntryDocumentMapper.ToDto(document);
    }

    public async Task<IReadOnlyList<AntenatalCheckupEntryDto>> ListByAntenatalRecordIdAsync(
        Guid antenatalRecordId,
        CancellationToken ct)
    {
        var documents = await database
            .GetCollection<AntenatalCheckupEntryDocument>(CollectionName)
            .Find(entry => entry.AntenatalRecordId == antenatalRecordId)
            .SortByDescending(entry => entry.RecordedAtUtc)
            .ToListAsync(ct);

        return documents.Select(AntenatalCheckupEntryDocumentMapper.ToDto).ToList();
    }
}
