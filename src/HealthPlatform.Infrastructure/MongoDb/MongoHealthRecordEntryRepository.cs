using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Infrastructure.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HealthPlatform.Infrastructure.MongoDb;

public sealed class MongoHealthRecordEntryRepository(IMongoDatabase database)
    : IHealthRecordEntryRepository
{
    private const string CollectionName = "health_record_entries";

    private IMongoCollection<HealthRecordEntryDocument> Collection =>
        database.GetCollection<HealthRecordEntryDocument>(CollectionName);

    public async Task<HealthRecordEntryDto> AddAsync(HealthRecordEntryCreateModel entry, CancellationToken ct)
    {
        var document = HealthRecordEntryDocumentMapper.ToDocument(entry);
        await Collection.InsertOneAsync(document, cancellationToken: ct);
        return HealthRecordEntryDocumentMapper.ToDto(document);
    }

    public async Task<HealthRecordEntryReference> AddTelemedicineSessionSummaryEntryAsync(
        HealthRecordTelemedicineSummaryEntry entry,
        CancellationToken ct)
    {
        var created = await AddAsync(
            new HealthRecordEntryCreateModel(
                entry.HealthRecordId,
                HealthRecordEntryType.TelemedicineSessionSummary,
                new HealthRecordEntryContentPayload(
                    TelemedicineSessionSummary: new TelemedicineSessionSummaryContent(
                        entry.SessionId,
                        entry.AppointmentId,
                        entry.DoctorId,
                        entry.SummaryDocumentId)),
                entry.DoctorId,
                entry.CreatedAtUtc,
                IsVisibleToPatient: true),
            ct);

        return new HealthRecordEntryReference(created.Id);
    }

    public async Task<HealthRecordEntryDto?> GetByIdAsync(string entryId, CancellationToken ct)
    {
        if (!ObjectId.TryParse(entryId, out var objectId))
        {
            return null;
        }

        var filter = Builders<HealthRecordEntryDocument>.Filter.And(
            Builders<HealthRecordEntryDocument>.Filter.Eq(document => document.Id, objectId),
            Builders<HealthRecordEntryDocument>.Filter.Eq(document => document.IsDeleted, false));

        var document = await Collection.Find(filter).FirstOrDefaultAsync(ct);
        return document is null ? null : HealthRecordEntryDocumentMapper.ToDto(document);
    }

    public async Task<IReadOnlyList<HealthRecordEntryDto>> ListByHealthRecordIdAsync(
        Guid healthRecordId,
        bool patientVisibleOnly,
        CancellationToken ct)
    {
        var filterBuilder = Builders<HealthRecordEntryDocument>.Filter;
        var filters = new List<FilterDefinition<HealthRecordEntryDocument>>
        {
            filterBuilder.Eq(document => document.HealthRecordId, healthRecordId),
            filterBuilder.Eq(document => document.IsDeleted, false)
        };

        if (patientVisibleOnly)
        {
            filters.Add(filterBuilder.Eq(document => document.IsVisibleToPatient, true));
        }

        var documents = await Collection
            .Find(filterBuilder.And(filters))
            .SortByDescending(document => document.CreatedAtUtc)
            .ToListAsync(ct);

        return documents.ConvertAll(HealthRecordEntryDocumentMapper.ToDto);
    }

    public async Task<bool> UpdateAsync(HealthRecordEntryUpdateModel entry, CancellationToken ct)
    {
        if (!ObjectId.TryParse(entry.EntryId, out var objectId))
        {
            return false;
        }

        var existing = await Collection
            .Find(document => document.Id == objectId && !document.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (existing is null)
        {
            return false;
        }

        var entryType = HealthRecordEntryDocumentMapper.ParseEntryType(existing.EntryType);
        var mergedContent = HealthRecordEntryContentResolver.MergeForUpdate(
            entryType,
            HealthRecordEntryDocumentMapper.FromBsonDocument(existing.Content, entryType),
            entry.Content);

        var updateBuilder = Builders<HealthRecordEntryDocument>.Update
            .Set(document => document.Content, HealthRecordEntryDocumentMapper.ToBsonDocument(mergedContent))
            .Set(document => document.UpdatedAtUtc, entry.UpdatedAtUtc);

        if (entry.IsVisibleToPatient.HasValue)
        {
            updateBuilder = updateBuilder.Set(
                document => document.IsVisibleToPatient,
                entry.IsVisibleToPatient.Value);
        }

        var result = await Collection.UpdateOneAsync(
            document => document.Id == objectId && !document.IsDeleted,
            updateBuilder,
            cancellationToken: ct);

        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string entryId, DateTime deletedAtUtc, CancellationToken ct)
    {
        if (!ObjectId.TryParse(entryId, out var objectId))
        {
            return false;
        }

        var result = await Collection.UpdateOneAsync(
            document => document.Id == objectId && !document.IsDeleted,
            Builders<HealthRecordEntryDocument>.Update
                .Set(document => document.IsDeleted, true)
                .Set(document => document.DeletedAt, deletedAtUtc),
            cancellationToken: ct);

        return result.ModifiedCount > 0;
    }
}
