using HealthPlatform.Application.Search;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingSearchService : ISearchService
{
    private IReadOnlyList<PharmacySearchMatchDto> _pharmacySearchResults = [];

    public List<Guid> DoctorUpserts { get; } = [];

    public List<Guid> DoctorRemovals { get; } = [];

    public List<Guid> PharmacyUpserts { get; } = [];

    public List<(Guid PharmacyId, IReadOnlyList<PharmacyStockIndexEntry> Stock)> PharmacyStockUpdates { get; } = [];

    public PharmacySearchCriteria? LastPharmacySearchCriteria { get; private set; }

    public LabPartnerSearchCriteria? LastLabPartnerSearchCriteria { get; private set; }

    public void SeedPharmacySearchResults(IReadOnlyList<PharmacySearchMatchDto> results) =>
        _pharmacySearchResults = results;

    public Task UpsertDoctorAsync(Guid doctorId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        DoctorUpserts.Add(doctorId);
        return Task.CompletedTask;
    }

    public Task RemoveDoctorAsync(Guid doctorId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        DoctorRemovals.Add(doctorId);
        return Task.CompletedTask;
    }

    public Task UpsertPharmacyAsync(Guid pharmacyId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        PharmacyUpserts.Add(pharmacyId);
        return Task.CompletedTask;
    }

    public Task UpdatePharmacyStockAsync(
        Guid pharmacyId,
        IReadOnlyList<PharmacyStockIndexEntry> stockSummary,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        PharmacyStockUpdates.Add((pharmacyId, stockSummary.ToList()));
        return Task.CompletedTask;
    }

    public Task<DoctorSearchPageDto> SearchDoctorsAsync(DoctorSearchCriteria criteria, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(new DoctorSearchPageDto([], 0));
    }

    public Task<PharmacySearchPageDto> SearchPharmaciesAsync(PharmacySearchCriteria criteria, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        LastPharmacySearchCriteria = criteria;

        var matches = _pharmacySearchResults.AsEnumerable();
        if (criteria.HasStock == true)
        {
            matches = matches.Where(match => match.HasStock);
        }

        var results = matches.ToList();
        var pageResults = results
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToList();

        return Task.FromResult(new PharmacySearchPageDto(pageResults, results.Count));
    }

    public Task<LabPartnerSearchPageDto> SearchLabPartnersAsync(LabPartnerSearchCriteria criteria, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        LastLabPartnerSearchCriteria = criteria;
        return Task.FromResult(new LabPartnerSearchPageDto([], 0));
    }
}
