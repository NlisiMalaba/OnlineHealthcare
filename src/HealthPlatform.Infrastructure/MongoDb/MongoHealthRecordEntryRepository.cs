using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Infrastructure.MongoDb.Documents;
using MongoDB.Driver;

namespace HealthPlatform.Infrastructure.MongoDb;

public sealed class MongoHealthRecordEntryRepository(IMongoDatabase database)
    : IHealthRecordEntryRepository
{
    private const string CollectionName = "health_record_entries";

    public async Task<HealthRecordEntryReference> AddTelemedicineSessionSummaryEntryAsync(
        HealthRecordTelemedicineSummaryEntry entry,
        CancellationToken ct)
    {
        var document = new HealthRecordEntryDocument
        {
            HealthRecordId = entry.HealthRecordId,
            EntryType = "telemedicine_session_summary",
            PatientId = entry.PatientId,
            DoctorId = entry.DoctorId,
            SessionId = entry.SessionId,
            AppointmentId = entry.AppointmentId,
            SummaryDocumentId = entry.SummaryDocumentId,
            CreatedAtUtc = entry.CreatedAtUtc,
            IsVisibleToPatient = true
        };

        await database
            .GetCollection<HealthRecordEntryDocument>(CollectionName)
            .InsertOneAsync(document, cancellationToken: ct);

        return new HealthRecordEntryReference(document.Id.ToString());
    }

    public async Task<HealthRecordEntryReference> AddReferralConsultationSummaryEntryAsync(
        HealthRecordReferralConsultationSummaryEntry entry,
        CancellationToken ct)
    {
        var document = new HealthRecordEntryDocument
        {
            HealthRecordId = entry.HealthRecordId,
            EntryType = "referral_consultation_summary",
            PatientId = entry.PatientId,
            DoctorId = entry.DoctorId,
            ReferralId = entry.ReferralId,
            ConsultationSummary = entry.Summary,
            CreatedAtUtc = entry.CreatedAtUtc,
            IsVisibleToPatient = true
        };

        await database
            .GetCollection<HealthRecordEntryDocument>(CollectionName)
            .InsertOneAsync(document, cancellationToken: ct);

        return new HealthRecordEntryReference(document.Id.ToString());
    }
}
