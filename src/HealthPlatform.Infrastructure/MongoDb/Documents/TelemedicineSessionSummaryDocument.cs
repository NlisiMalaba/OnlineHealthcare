using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HealthPlatform.Infrastructure.MongoDb.Documents;

public sealed class TelemedicineSessionSummaryDocument
{
    [BsonId]
    public ObjectId Id { get; set; }

    public Guid SessionId { get; set; }

    public Guid AppointmentId { get; set; }

    public Guid PatientId { get; set; }

    public Guid DoctorId { get; set; }

    public string Mode { get; set; } = string.Empty;

    public int DurationSeconds { get; set; }

    public DateTime StartedAtUtc { get; set; }

    public DateTime EndedAtUtc { get; set; }

    public bool RecordingEnabled { get; set; }

    public string SummaryText { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}
