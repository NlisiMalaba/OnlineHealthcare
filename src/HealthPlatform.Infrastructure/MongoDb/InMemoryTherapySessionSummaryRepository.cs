using HealthPlatform.Application.MentalHealth;

namespace HealthPlatform.Infrastructure.MongoDb;

public sealed class InMemoryTherapySessionSummaryRepository : ITherapySessionSummaryRepository
{
    public List<TherapySessionSummaryRecord> Summaries { get; } = [];

    public Task<TherapySessionSummaryReference> SaveAsync(
        TherapySessionSummaryRecord summary,
        CancellationToken ct)
    {
        Summaries.Add(summary);
        return Task.FromResult(new TherapySessionSummaryReference(Guid.CreateVersion7().ToString("N")));
    }
}
