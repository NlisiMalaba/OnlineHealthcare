using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HealthPlatform.Infrastructure.MongoDb.Documents;

public sealed class HealthRecordEntryDocument
{
    [BsonId]
    public ObjectId Id { get; set; }

    public Guid HealthRecordId { get; set; }

    public string EntryType { get; set; } = string.Empty;

    public BsonDocument Content { get; set; } = [];

    public Guid AuthoredBy { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public bool IsVisibleToPatient { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}
