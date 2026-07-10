using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HealthPlatform.Infrastructure.MongoDb.Documents;

public sealed class MoodLogDocument
{
    [BsonId]
    public ObjectId Id { get; set; }

    public Guid PatientId { get; set; }

    public int Rating { get; set; }

    public string? Notes { get; set; }

    public DateTime LoggedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAtUtc { get; set; }
}
