using HealthPlatform.Application.HealthRecords;

namespace HealthPlatform.Infrastructure.MongoDb;

public sealed class InMemoryHealthRecordEntryRepository : IHealthRecordEntryRepository
{
    public List<HealthRecordTelemedicineSummaryEntry> Entries { get; } = [];
    public List<HealthRecordReferralConsultationSummaryEntry> ReferralSummaries { get; } = [];

    public Task<HealthRecordEntryReference> AddTelemedicineSessionSummaryEntryAsync(
        HealthRecordTelemedicineSummaryEntry entry,
        CancellationToken ct)
    {
        Entries.Add(entry);
        return Task.FromResult(new HealthRecordEntryReference(Guid.CreateVersion7().ToString("N")));
    }

    public Task<HealthRecordEntryReference> AddReferralConsultationSummaryEntryAsync(
        HealthRecordReferralConsultationSummaryEntry entry,
        CancellationToken ct)
    {
        ReferralSummaries.Add(entry);
        return Task.FromResult(new HealthRecordEntryReference(Guid.CreateVersion7().ToString("N")));
    }
}
