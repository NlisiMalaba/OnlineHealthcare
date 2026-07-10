using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HealthPlatform.Infrastructure.MongoDb.Documents;

public sealed class AntenatalCheckupEntryDocument
{
    [BsonId]
    public ObjectId Id { get; set; }

    public Guid AntenatalRecordId { get; set; }

    public Guid? ScheduleEntryId { get; set; }

    public Guid PatientId { get; set; }

    public Guid DoctorId { get; set; }

    public int GestationalAgeWeeks { get; set; }

    public int? FetalHeartRateBpm { get; set; }

    public decimal? FundalHeightCm { get; set; }

    public decimal? EstimatedFetalWeightGrams { get; set; }

    public int? BloodPressureSystolic { get; set; }

    public int? BloodPressureDiastolic { get; set; }

    public decimal? MaternalWeightKg { get; set; }

    public string? ClinicalNotes { get; set; }

    public DateTime RecordedAtUtc { get; set; }
}
