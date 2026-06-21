using HealthPlatform.Application.Search;
using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingSearchService : ISearchService
{
    public List<(Guid DoctorId, IReadOnlyList<DoctorAvailabilityIndexEntry> Slots)> AvailabilityUpdates { get; } = [];

    public Task UpdateDoctorAvailabilityIndexAsync(
        Guid doctorId,
        IReadOnlyList<DoctorAvailabilityIndexEntry> availabilitySlots,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        AvailabilityUpdates.Add((doctorId, availabilitySlots.ToList()));
        return Task.CompletedTask;
    }
}
