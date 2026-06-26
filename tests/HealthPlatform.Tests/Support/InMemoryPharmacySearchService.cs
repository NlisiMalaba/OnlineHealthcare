using HealthPlatform.Application.Search;
using HealthPlatform.Domain.ValueObjects;
using HealthPlatform.Tests.Arbitraries;

namespace HealthPlatform.Tests.Support;

/// <summary>
/// In-memory pharmacy search that applies stock filters without Elasticsearch.
/// Used to validate pharmacy stock filter invariants in property tests.
/// </summary>
public sealed class InMemoryPharmacySearchService : ISearchService
{
    private readonly List<IndexedPharmacyForSearch> _pharmacies = [];

    public void Reset() => _pharmacies.Clear();

    public void Seed(IEnumerable<IndexedPharmacyForSearch> pharmacies) => _pharmacies.AddRange(pharmacies);

    public Task<PharmacySearchPageDto> SearchPharmaciesAsync(PharmacySearchCriteria criteria, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var hasGeo = criteria.PatientLatitude.HasValue && criteria.PatientLongitude.HasValue;
        var patientLocation = hasGeo
            ? new GeoPoint(criteria.PatientLatitude!.Value, criteria.PatientLongitude!.Value)
            : null;

        var matches = _pharmacies
            .Where(pharmacy => PharmacySearchMatcher.Matches(pharmacy, criteria))
            .Select(pharmacy =>
            {
                double? distanceKm = patientLocation is null || pharmacy.Location is null
                    ? null
                    : GeoDistanceCalculator.KilometersBetween(patientLocation, pharmacy.Location);

                var hasStock = pharmacy.StockSummary.Any(line => line.QuantityOnHand > 0);

                return new PharmacySearchMatchDto(
                    pharmacy.PharmacyId,
                    pharmacy.Name,
                    pharmacy.Address,
                    hasStock,
                    distanceKm);
            })
            .ToList();

        if (hasGeo)
        {
            matches = matches
                .OrderBy(match => match.DistanceKilometers ?? double.MaxValue)
                .ThenBy(match => match.PharmacyId)
                .ToList();
        }
        else
        {
            matches = matches
                .OrderBy(match => match.Name, StringComparer.Ordinal)
                .ThenBy(match => match.PharmacyId)
                .ToList();
        }

        var pageResults = matches
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToList();

        return Task.FromResult(new PharmacySearchPageDto(pageResults, matches.Count));
    }

    public Task UpsertDoctorAsync(Guid doctorId, CancellationToken ct) => Task.CompletedTask;

    public Task RemoveDoctorAsync(Guid doctorId, CancellationToken ct) => Task.CompletedTask;

    public Task UpsertPharmacyAsync(Guid pharmacyId, CancellationToken ct) => Task.CompletedTask;

    public Task UpdatePharmacyStockAsync(
        Guid pharmacyId,
        IReadOnlyList<PharmacyStockIndexEntry> stockSummary,
        CancellationToken ct) => Task.CompletedTask;

    public Task<DoctorSearchPageDto> SearchDoctorsAsync(DoctorSearchCriteria criteria, CancellationToken ct) =>
        Task.FromResult(new DoctorSearchPageDto([], 0));

    public Task<LabPartnerSearchPageDto> SearchLabPartnersAsync(LabPartnerSearchCriteria criteria, CancellationToken ct) =>
        Task.FromResult(new LabPartnerSearchPageDto([], 0));
}

internal static class PharmacySearchMatcher
{
    public static bool Matches(IndexedPharmacyForSearch pharmacy, PharmacySearchCriteria criteria)
    {
        if (!pharmacy.IsSearchable)
        {
            return false;
        }

        if (criteria.HasStock == true
            && !pharmacy.StockSummary.Any(line => line.QuantityOnHand > 0))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(criteria.MedicationSku)
            && !HasMedicationInStock(pharmacy, criteria.MedicationSku))
        {
            return false;
        }

        return true;
    }

    public static bool HasMedicationInStock(IndexedPharmacyForSearch pharmacy, string medicationSku) =>
        pharmacy.StockSummary.Any(line =>
            string.Equals(line.MedicationSku, medicationSku, StringComparison.Ordinal)
            && line.QuantityOnHand > 0);
}
