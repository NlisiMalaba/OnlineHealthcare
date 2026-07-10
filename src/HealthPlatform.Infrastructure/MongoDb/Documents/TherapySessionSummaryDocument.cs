using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HealthPlatform.Infrastructure.MongoDb.Documents;

public sealed class TherapySessionSummaryDocument
{
    [BsonId]
    public ObjectId Id { get; set; }

    public Guid TherapySessionId { get; set; }

    public Guid AppointmentId { get; set; }

    public Guid PatientId { get; set; }

    public Guid TherapistId { get; set; }

    public string SummaryText { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}
