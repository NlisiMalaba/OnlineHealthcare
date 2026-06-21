using HealthPlatform.Application.Search;
using HealthPlatform.Domain.ValueObjects;
using HealthPlatform.Tests.Arbitraries;

namespace HealthPlatform.Tests.Support;

/// <summary>
/// In-memory doctor search that sorts by haversine distance when patient coordinates are supplied.
/// Used to validate proximity ordering invariants without requiring Elasticsearch in property tests.
/// </summary>
public sealed class HaversineDoctorSearchService : ISearchService
{
    private readonly List<IndexedDoctorForSearch> _doctors = [];

    public void Reset() => _doctors.Clear();

    public void Seed(IEnumerable<IndexedDoctorForSearch> doctors) => _doctors.AddRange(doctors);

    public Task<DoctorSearchPageDto> SearchDoctorsAsync(DoctorSearchCriteria criteria, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        IEnumerable<IndexedDoctorForSearch> matches = _doctors;

        if (!string.IsNullOrWhiteSpace(criteria.Specialty))
        {
            matches = matches.Where(doctor =>
                string.Equals(doctor.Specialty, criteria.Specialty, StringComparison.Ordinal));
        }

        if (criteria.MinRating.HasValue)
        {
            matches = matches.Where(doctor => doctor.AverageRating >= criteria.MinRating.Value);
        }

        if (criteria.MinFee.HasValue)
        {
            var minFee = criteria.MinFee.Value;
            matches = matches.Where(doctor =>
                Math.Max(doctor.VirtualFee, doctor.PhysicalFee) >= minFee);
        }

        if (criteria.MaxFee.HasValue)
        {
            var maxFee = criteria.MaxFee.Value;
            matches = matches.Where(doctor =>
                Math.Min(doctor.VirtualFee, doctor.PhysicalFee) <= maxFee);
        }

        if (criteria.HasAvailability == true)
        {
            matches = matches.Where(doctor => doctor.HasAvailability);
        }

        var hasGeo = criteria.PatientLatitude.HasValue && criteria.PatientLongitude.HasValue;
        var patientLocation = hasGeo
            ? new GeoPoint(criteria.PatientLatitude!.Value, criteria.PatientLongitude!.Value)
            : null;

        var ranked = matches
            .Select(doctor =>
            {
                double? distanceKm = patientLocation is null
                    ? null
                    : GeoDistanceCalculator.KilometersBetween(patientLocation, doctor.ClinicLocation);

                return new DoctorSearchMatchDto(
                    doctor.DoctorId,
                    $"Doctor {doctor.DoctorId:N}",
                    doctor.Specialty,
                    (decimal)doctor.AverageRating,
                    0,
                    doctor.VirtualFee,
                    doctor.PhysicalFee,
                    distanceKm);
            })
            .ToList();

        if (hasGeo)
        {
            ranked = ranked
                .OrderBy(result => result.DistanceKilometers ?? double.MaxValue)
                .ThenBy(result => result.DoctorId)
                .ToList();
        }
        else
        {
            ranked = ranked
                .OrderByDescending(result => result.AverageRating)
                .ThenBy(result => result.Name, StringComparer.Ordinal)
                .ToList();
        }

        var pageResults = ranked
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToList();

        return Task.FromResult(new DoctorSearchPageDto(pageResults, ranked.Count));
    }

    public Task UpsertDoctorAsync(Guid doctorId, CancellationToken ct) => Task.CompletedTask;

    public Task RemoveDoctorAsync(Guid doctorId, CancellationToken ct) => Task.CompletedTask;

    public Task UpsertPharmacyAsync(Guid pharmacyId, CancellationToken ct) => Task.CompletedTask;

    public Task UpdatePharmacyStockAsync(
        Guid pharmacyId,
        IReadOnlyList<PharmacyStockIndexEntry> stockSummary,
        CancellationToken ct) => Task.CompletedTask;
}
