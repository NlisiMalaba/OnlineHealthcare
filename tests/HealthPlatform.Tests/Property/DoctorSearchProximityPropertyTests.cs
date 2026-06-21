using FsCheck;
using FsCheck.Xunit;
using HealthPlatform.Application.Search;
using HealthPlatform.Application.Search.SearchDoctors;
using HealthPlatform.Domain.ValueObjects;
using HealthPlatform.Infrastructure.Search;
using HealthPlatform.Infrastructure.Search.Documents;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;

namespace HealthPlatform.Tests.Properties;

public sealed class DoctorSearchProximityPropertyTests
{
    private const double DistanceToleranceKilometers = 0.01;

    // Feature: online-healthcare-platform, Property 6: Doctor Search Proximity Ordering
    [Property(Arbitrary = [typeof(DoctorSearchArbitraries)], MaxTest = 100)]
    public bool Proximity_search_results_are_sorted_by_ascending_distance(DoctorProximitySearchCase searchCase)
    {
        var searchService = new HaversineDoctorSearchService();
        searchService.Seed(searchCase.Doctors);

        var handler = new SearchDoctorsQueryHandler(searchService);
        var response = handler.Handle(
            new SearchDoctorsQuery(
                PatientLatitude: searchCase.PatientLocation.Latitude,
                PatientLongitude: searchCase.PatientLocation.Longitude,
                PageSize: searchCase.Doctors.Count),
            CancellationToken.None).GetAwaiter().GetResult();

        if (response.Results.Count <= 1)
        {
            return true;
        }

        if (!DoctorSearchProximityOrdering.IsAscendingByDistance(
                response.Results
                    .Select(result => new DoctorSearchMatchDto(
                        result.DoctorId,
                        result.Name,
                        result.Specialty,
                        result.AverageRating,
                        result.TotalReviews,
                        result.VirtualFee,
                        result.PhysicalFee,
                        result.DistanceKilometers))
                    .ToList()))
        {
            return false;
        }

        foreach (var result in response.Results)
        {
            var doctor = searchCase.Doctors.Single(item => item.DoctorId == result.DoctorId);
            var expectedDistance = GeoDistanceCalculator.KilometersBetween(
                searchCase.PatientLocation,
                doctor.ClinicLocation);

            if (!result.DistanceKilometers.HasValue
                || Math.Abs(result.DistanceKilometers.Value - expectedDistance) > DistanceToleranceKilometers)
            {
                return false;
            }
        }

        return true;
    }

    // Feature: online-healthcare-platform, Property 6: Doctor Search Proximity Ordering
    [Property(Arbitrary = [typeof(DoctorSearchArbitraries)], MaxTest = 100)]
    public bool Elasticsearch_geo_search_request_uses_ascending_distance_sort(DoctorProximitySearchCase searchCase)
    {
        var criteria = new DoctorSearchCriteria(
            Specialty: null,
            MinRating: null,
            MinFee: null,
            MaxFee: null,
            HasAvailability: null,
            PatientLatitude: searchCase.PatientLocation.Latitude,
            PatientLongitude: searchCase.PatientLocation.Longitude,
            Page: 1,
            PageSize: searchCase.Doctors.Count);

        var requestBody = DoctorElasticsearchSearchSupport.BuildSearchRequestBody(criteria, from: 0, hasGeo: true);

        return requestBody.Contains("_geo_distance", StringComparison.Ordinal)
            && requestBody.Contains("\"order\"", StringComparison.Ordinal)
            && requestBody.Contains("asc", StringComparison.Ordinal)
            && requestBody.Contains("\"unit\"", StringComparison.Ordinal)
            && requestBody.Contains("km", StringComparison.Ordinal);
    }

    // Feature: online-healthcare-platform, Property 6: Doctor Search Proximity Ordering
    [Property(Arbitrary = [typeof(DoctorSearchArbitraries)], MaxTest = 100)]
    public bool Elasticsearch_geo_search_response_preserves_ascending_distance_order(
        DoctorProximitySearchCase searchCase)
    {
        var hits = searchCase.Doctors
            .Select(doctor =>
            {
                var distance = GeoDistanceCalculator.KilometersBetween(
                    searchCase.PatientLocation,
                    doctor.ClinicLocation);

                var document = new DoctorSearchDocument
                {
                    DoctorId = doctor.DoctorId.ToString(),
                    Name = $"Doctor {doctor.DoctorId:N}",
                    Specialty = doctor.Specialty,
                    AverageRating = doctor.AverageRating,
                    TotalReviews = 0,
                    VirtualFee = doctor.VirtualFee,
                    PhysicalFee = doctor.PhysicalFee,
                    MinFee = Math.Min(doctor.VirtualFee, doctor.PhysicalFee),
                    MaxFee = Math.Max(doctor.VirtualFee, doctor.PhysicalFee),
                    HasAvailability = doctor.HasAvailability,
                    IsSearchable = true
                };

                return (document, distance);
            })
            .OrderBy(hit => hit.distance)
            .ThenBy(hit => hit.document.DoctorId, StringComparer.Ordinal)
            .ToList();

        var responseBody = DoctorElasticsearchSearchSupport.BuildSimulatedSearchResponse(hits);
        var parsed = DoctorElasticsearchSearchSupport.ParseSearchResponse(responseBody, hasGeo: true);

        return DoctorSearchProximityOrdering.IsAscendingByDistance(parsed.Results);
    }
}
