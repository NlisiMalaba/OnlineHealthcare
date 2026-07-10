using HealthPlatform.Application.MentalHealth;
using HealthPlatform.Infrastructure.MongoDb.Documents;
using MongoDB.Driver;

namespace HealthPlatform.Infrastructure.MongoDb;

public sealed class MongoTherapySessionSummaryRepository(IMongoDatabase database)
    : ITherapySessionSummaryRepository
{
    private const string CollectionName = "therapy_session_summaries";

    public async Task<TherapySessionSummaryReference> SaveAsync(
        TherapySessionSummaryRecord summary,
        CancellationToken ct)
    {
        var document = new TherapySessionSummaryDocument
        {
            TherapySessionId = summary.TherapySessionId,
            AppointmentId = summary.AppointmentId,
            PatientId = summary.PatientId,
            TherapistId = summary.TherapistId,
            SummaryText = summary.SummaryText,
            CreatedAtUtc = summary.CreatedAtUtc
        };

        await database
            .GetCollection<TherapySessionSummaryDocument>(CollectionName)
            .InsertOneAsync(document, cancellationToken: ct);

        return new TherapySessionSummaryReference(document.Id.ToString());
    }
}
