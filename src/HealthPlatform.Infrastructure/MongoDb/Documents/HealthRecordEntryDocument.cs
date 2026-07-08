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

    public Guid PatientId { get; set; }

    public Guid DoctorId { get; set; }

    public Guid AuthoredBy { get; set; }

    public Guid AppointmentId { get; set; }

    public string? SummaryDocumentId { get; set; }

    public Guid? ReferralId { get; set; }

    public string? ConsultationSummary { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public bool IsVisibleToPatient { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}
