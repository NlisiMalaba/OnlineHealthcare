using HealthPlatform.Application.MentalHealth.MoodLogs;
using HealthPlatform.Infrastructure.MongoDb.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HealthPlatform.Infrastructure.MongoDb;

public sealed class MongoMoodLogRepository(IMongoDatabase database) : IMoodLogRepository
{
    private const string CollectionName = "mood_logs";

    private IMongoCollection<MoodLogDocument> Collection =>
        database.GetCollection<MoodLogDocument>(CollectionName);

    public async Task<MoodLogDto> AddAsync(MoodLogCreateModel entry, CancellationToken ct)
    {
        var document = MoodLogDocumentMapper.ToDocument(entry);
        await Collection.InsertOneAsync(document, cancellationToken: ct);
        return MoodLogDocumentMapper.ToDto(document);
    }

    public async Task<MoodLogDto?> GetByIdAsync(string id, CancellationToken ct)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return null;
        }

        var document = await Collection
            .Find(log => log.Id == objectId && !log.IsDeleted)
            .FirstOrDefaultAsync(ct);

        return document is null ? null : MoodLogDocumentMapper.ToDto(document);
    }

    public async Task<MoodLogDto?> GetByIdForPatientAsync(string id, Guid patientId, CancellationToken ct)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return null;
        }

        var document = await Collection
            .Find(log => log.Id == objectId && log.PatientId == patientId && !log.IsDeleted)
            .FirstOrDefaultAsync(ct);

        return document is null ? null : MoodLogDocumentMapper.ToDto(document);
    }

    public async Task<IReadOnlyList<MoodLogDto>> ListByPatientIdAsync(
        Guid patientId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken ct)
    {
        var filterBuilder = Builders<MoodLogDocument>.Filter;
        var filters = new List<FilterDefinition<MoodLogDocument>>
        {
            filterBuilder.Eq(log => log.PatientId, patientId),
            filterBuilder.Eq(log => log.IsDeleted, false)
        };

        if (fromUtc.HasValue)
        {
            filters.Add(filterBuilder.Gte(log => log.LoggedAtUtc, fromUtc.Value));
        }

        if (toUtc.HasValue)
        {
            filters.Add(filterBuilder.Lte(log => log.LoggedAtUtc, toUtc.Value));
        }

        var documents = await Collection
            .Find(filterBuilder.And(filters))
            .SortByDescending(log => log.LoggedAtUtc)
            .ToListAsync(ct);

        return documents.ConvertAll(MoodLogDocumentMapper.ToDto);
    }

    public async Task<bool> UpdateAsync(MoodLogUpdateModel update, CancellationToken ct)
    {
        if (!ObjectId.TryParse(update.Id, out var objectId))
        {
            return false;
        }

        var result = await Collection.UpdateOneAsync(
            log => log.Id == objectId && !log.IsDeleted,
            Builders<MoodLogDocument>.Update
                .Set(log => log.Rating, update.Rating)
                .Set(log => log.Notes, update.Notes)
                .Set(log => log.LoggedAtUtc, update.LoggedAtUtc)
                .Set(log => log.UpdatedAtUtc, update.UpdatedAtUtc),
            cancellationToken: ct);

        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, DateTime deletedAtUtc, CancellationToken ct)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return false;
        }

        var result = await Collection.UpdateOneAsync(
            log => log.Id == objectId && !log.IsDeleted,
            Builders<MoodLogDocument>.Update
                .Set(log => log.IsDeleted, true)
                .Set(log => log.DeletedAtUtc, deletedAtUtc),
            cancellationToken: ct);

        return result.ModifiedCount > 0;
    }
}
