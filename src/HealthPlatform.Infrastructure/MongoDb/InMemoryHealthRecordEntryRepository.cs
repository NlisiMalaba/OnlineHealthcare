using HealthPlatform.Application.HealthRecords;

namespace HealthPlatform.Infrastructure.MongoDb;

public sealed class InMemoryHealthRecordEntryRepository : IHealthRecordEntryRepository
{
    public List<HealthRecordTelemedicineSummaryEntry> Entries { get; } = [];

    public Task<HealthRecordEntryReference> AddTelemedicineSessionSummaryEntryAsync(
        HealthRecordTelemedicineSummaryEntry entry,
        CancellationToken ct)
    {
        Entries.Add(entry);
        return Task.FromResult(new HealthRecordEntryReference(Guid.CreateVersion7().ToString("N")));
    }
}
