using HealthPlatform.Application.Search;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Search;

public sealed class LoggingSearchService(ILogger<LoggingSearchService> logger) : ISearchService
{
    public Task UpsertDoctorAsync(Guid doctorId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation("Doctor search index upsert requested for doctor {DoctorId}.", doctorId);
        return Task.CompletedTask;
    }

    public Task RemoveDoctorAsync(Guid doctorId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation("Doctor search index removal requested for doctor {DoctorId}.", doctorId);
        return Task.CompletedTask;
    }

    public Task UpsertPharmacyAsync(Guid pharmacyId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation("Pharmacy search index upsert requested for pharmacy {PharmacyId}.", pharmacyId);
        return Task.CompletedTask;
    }

    public Task UpdatePharmacyStockAsync(
        Guid pharmacyId,
        IReadOnlyList<PharmacyStockIndexEntry> stockSummary,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Pharmacy stock search index update requested for pharmacy {PharmacyId} with {ItemCount} items.",
            pharmacyId,
            stockSummary.Count);
        return Task.CompletedTask;
    }
}
