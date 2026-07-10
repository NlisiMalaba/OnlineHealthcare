using HealthPlatform.Application.MentalHealth.MoodLogs;
using HealthPlatform.Infrastructure.MongoDb.Documents;

namespace HealthPlatform.Infrastructure.MongoDb;

internal static class MoodLogDocumentMapper
{
    public static MoodLogDto ToDto(MoodLogDocument document) =>
        new(
            document.Id.ToString(),
            document.PatientId,
            document.Rating,
            document.Notes,
            document.LoggedAtUtc,
            document.CreatedAtUtc,
            document.UpdatedAtUtc);

    public static MoodLogDocument ToDocument(MoodLogCreateModel entry) =>
        new()
        {
            PatientId = entry.PatientId,
            Rating = entry.Rating,
            Notes = entry.Notes,
            LoggedAtUtc = entry.LoggedAtUtc,
            CreatedAtUtc = entry.CreatedAtUtc,
            IsDeleted = false
        };
}
