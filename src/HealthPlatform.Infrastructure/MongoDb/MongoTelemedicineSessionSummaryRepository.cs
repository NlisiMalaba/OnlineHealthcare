using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Infrastructure.MongoDb.Documents;
using MongoDB.Driver;

namespace HealthPlatform.Infrastructure.MongoDb;

public sealed class MongoTelemedicineSessionSummaryRepository(IMongoDatabase database)
    : ITelemedicineSessionSummaryRepository
{
    private const string CollectionName = "telemedicine_session_summaries";

    public async Task<TelemedicineSessionSummaryReference> SaveAsync(
        TelemedicineSessionSummaryRecord summary,
        CancellationToken ct)
    {
        var document = new TelemedicineSessionSummaryDocument
        {
            SessionId = summary.SessionId,
            AppointmentId = summary.AppointmentId,
            PatientId = summary.PatientId,
            DoctorId = summary.DoctorId,
            Mode = summary.Mode.ToString(),
            DurationSeconds = summary.DurationSeconds,
            StartedAtUtc = summary.StartedAtUtc,
            EndedAtUtc = summary.EndedAtUtc,
            RecordingEnabled = summary.RecordingEnabled,
            SummaryText = summary.SummaryText,
            CreatedAtUtc = DateTime.UtcNow
        };

        await database
            .GetCollection<TelemedicineSessionSummaryDocument>(CollectionName)
            .InsertOneAsync(document, cancellationToken: ct);

        return new TelemedicineSessionSummaryReference(document.Id.ToString());
    }
}
