using FsCheck.Xunit;
using HealthPlatform.Application.Search;
using HealthPlatform.Application.Search.SearchPharmacies;
using HealthPlatform.Infrastructure.Search;
using HealthPlatform.Infrastructure.Search.Documents;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;

namespace HealthPlatform.Tests.Properties;

public sealed class PharmacyStockFilterPropertyTests
{
    // Feature: online-healthcare-platform, Property 13: Pharmacy Stock Filter
    [Property(Arbitrary = [typeof(PharmacyStockSearchArbitraries)], MaxTest = 100)]
    public bool Medication_order_search_returns_only_pharmacies_with_requested_medication_in_stock(
        PharmacyStockSearchCase searchCase)
    {
        var searchService = new InMemoryPharmacySearchService();
        searchService.Seed(searchCase.Pharmacies);

        var handler = new SearchPharmaciesQueryHandler(searchService);
        var response = handler.Handle(
            new SearchPharmaciesQuery(
                MedicationSku: searchCase.MedicationSku,
                HasStock: true,
                PatientLatitude: searchCase.PatientLocation?.Latitude,
                PatientLongitude: searchCase.PatientLocation?.Longitude,
                PageSize: searchCase.Pharmacies.Count),
            CancellationToken.None).GetAwaiter().GetResult();

        foreach (var result in response.Results)
        {
            var pharmacy = searchCase.Pharmacies.Single(item => item.PharmacyId == result.PharmacyId);
            if (!PharmacySearchMatcher.HasMedicationInStock(pharmacy, searchCase.MedicationSku))
            {
                return false;
            }

            if (!result.HasStock)
            {
                return false;
            }
        }

        return true;
    }

    // Feature: online-healthcare-platform, Property 13: Pharmacy Stock Filter
    [Property(Arbitrary = [typeof(PharmacyStockSearchArbitraries)], MaxTest = 100)]
    public bool Elasticsearch_medication_search_request_requires_positive_stock_for_requested_sku(
        PharmacyStockSearchCase searchCase)
    {
        var criteria = new PharmacySearchCriteria(
            searchCase.MedicationSku,
            HasStock: true,
            searchCase.PatientLocation?.Latitude,
            searchCase.PatientLocation?.Longitude,
            Page: 1,
            PageSize: searchCase.Pharmacies.Count);

        var hasGeo = searchCase.PatientLocation is not null;
        var requestBody = PharmacyElasticsearchSearchSupport.BuildSearchRequestBody(criteria, from: 0, hasGeo);

        return requestBody.Contains("stockSummary.medicationSku", StringComparison.Ordinal)
            && requestBody.Contains(searchCase.MedicationSku, StringComparison.Ordinal)
            && requestBody.Contains("stockSummary.quantityOnHand", StringComparison.Ordinal)
            && requestBody.Contains("\"gt\":0", StringComparison.Ordinal)
            && requestBody.Contains("\"hasStock\":true", StringComparison.Ordinal);
    }

    // Feature: online-healthcare-platform, Property 13: Pharmacy Stock Filter
    [Property(Arbitrary = [typeof(PharmacyStockSearchArbitraries)], MaxTest = 100)]
    public bool Elasticsearch_simulated_response_only_contains_pharmacies_matching_stock_filter(
        PharmacyStockSearchCase searchCase)
    {
        var criteria = new PharmacySearchCriteria(
            searchCase.MedicationSku,
            HasStock: true,
            searchCase.PatientLocation?.Latitude,
            searchCase.PatientLocation?.Longitude,
            Page: 1,
            PageSize: searchCase.Pharmacies.Count);

        var hasGeo = searchCase.PatientLocation is not null;
        var matchingPharmacies = searchCase.Pharmacies
            .Where(pharmacy => PharmacySearchMatcher.Matches(pharmacy, criteria))
            .ToList();

        var hits = matchingPharmacies
            .Select(pharmacy =>
            {
                var distance = hasGeo && searchCase.PatientLocation is not null && pharmacy.Location is not null
                    ? GeoDistanceCalculator.KilometersBetween(searchCase.PatientLocation, pharmacy.Location)
                    : 0d;

                var document = new PharmacySearchDocument
                {
                    PharmacyId = pharmacy.PharmacyId.ToString(),
                    Name = pharmacy.Name,
                    Address = pharmacy.Address,
                    HasStock = pharmacy.StockSummary.Any(line => line.QuantityOnHand > 0),
                    IsSearchable = pharmacy.IsSearchable,
                    StockSummary = pharmacy.StockSummary
                        .Select(line => new PharmacyStockSummaryEntry
                        {
                            MedicationSku = line.MedicationSku,
                            MedicationName = line.MedicationSku,
                            QuantityOnHand = line.QuantityOnHand
                        })
                        .ToList()
                };

                return (document, distance);
            })
            .OrderBy(hit => hit.distance)
            .ThenBy(hit => hit.document.PharmacyId, StringComparer.Ordinal)
            .ToList();

        var responseBody = PharmacyElasticsearchSearchSupport.BuildSimulatedSearchResponse(hits);
        var parsed = PharmacyElasticsearchSearchSupport.ParseSearchResponse(responseBody, hasGeo);

        foreach (var result in parsed.Results)
        {
            var pharmacy = searchCase.Pharmacies.Single(item => item.PharmacyId == result.PharmacyId);
            if (!PharmacySearchMatcher.HasMedicationInStock(pharmacy, searchCase.MedicationSku))
            {
                return false;
            }
        }

        return parsed.Results.Count == matchingPharmacies.Count;
    }
}
