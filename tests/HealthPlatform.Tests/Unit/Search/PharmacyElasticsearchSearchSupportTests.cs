using HealthPlatform.Application.Search;
using HealthPlatform.Infrastructure.Search;
using Xunit;

namespace HealthPlatform.Tests.Unit.Search;

public sealed class PharmacyElasticsearchSearchSupportTests
{
    [Fact]
    public void BuildSearchRequestBody_WithMedicationSkuAndGeo_IncludesNestedStockAndGeoSort()
    {
        var criteria = new PharmacySearchCriteria(
            "MED-001",
            HasStock: true,
            PatientLatitude: -17.8,
            PatientLongitude: 31.0,
            Page: 1,
            PageSize: 20);

        var requestBody = PharmacyElasticsearchSearchSupport.BuildSearchRequestBody(criteria, from: 0, hasGeo: true);

        Assert.Contains("stockSummary.medicationSku", requestBody, StringComparison.Ordinal);
        Assert.Contains("stockSummary.quantityOnHand", requestBody, StringComparison.Ordinal);
        Assert.Contains("_geo_distance", requestBody, StringComparison.Ordinal);
        Assert.Contains("\"hasStock\":true", requestBody, StringComparison.Ordinal);
    }

    [Fact]
    public void ParseSearchResponse_WithGeoSort_PopulatesDistanceKilometers()
    {
        var pharmacyId = Guid.NewGuid();
        var responseBody = $$"""
            {
              "hits": {
                "total": { "value": 1 },
                "hits": [
                  {
                    "sort": [2.5],
                    "_source": {
                      "pharmacyId": "{{pharmacyId}}",
                      "name": "City Pharmacy",
                      "address": "123 Main Street",
                      "hasStock": true
                    }
                  }
                ]
              }
            }
            """;

        var parsed = PharmacyElasticsearchSearchSupport.ParseSearchResponse(responseBody, hasGeo: true);

        Assert.Single(parsed.Results);
        Assert.Equal(pharmacyId, parsed.Results[0].PharmacyId);
        Assert.Equal(2.5, parsed.Results[0].DistanceKilometers);
    }
}
