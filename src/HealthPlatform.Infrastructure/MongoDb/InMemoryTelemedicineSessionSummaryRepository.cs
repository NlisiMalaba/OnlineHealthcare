using HealthPlatform.Application.HealthRecords;

namespace HealthPlatform.Infrastructure.MongoDb;

public sealed class InMemoryTelemedicineSessionSummaryRepository
    : ITelemedicineSessionSummaryRepository
{
    public List<TelemedicineSessionSummaryRecord> Summaries { get; } = [];

    public Task<TelemedicineSessionSummaryReference> SaveAsync(
        TelemedicineSessionSummaryRecord summary,
        CancellationToken ct)
    {
        Summaries.Add(summary);
        return Task.FromResult(new TelemedicineSessionSummaryReference(Guid.CreateVersion7().ToString("N")));
    }
}
