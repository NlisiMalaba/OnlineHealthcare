using HealthPlatform.Application.Search;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Search;

/// <summary>
/// Development-oriented stub for Elasticsearch doctor indexing; replace with NEST/Elastic.Clients client in production.
/// </summary>
public sealed class LoggingSearchService(ILogger<LoggingSearchService> logger) : ISearchService
{
    public Task UpdateDoctorAvailabilityIndexAsync(
        Guid doctorId,
        IReadOnlyList<DoctorAvailabilityIndexEntry> availabilitySlots,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Doctor availability index update requested for doctor {DoctorId} with {SlotCount} slots.",
            doctorId,
            availabilitySlots.Count);
        return Task.CompletedTask;
    }
}
