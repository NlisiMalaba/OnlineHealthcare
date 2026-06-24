using HealthPlatform.Application.Search;
using HealthPlatform.Infrastructure.Search;
using Xunit;

namespace HealthPlatform.Tests.Unit.Search;

public sealed class LabPartnerElasticsearchSearchSupportTests
{
    [Fact]
    public void BuildSearchRequestBody_WithTestTypeAndPriceRange_IncludesNestedPricingFilters()
    {
        var criteria = new LabPartnerSearchCriteria(
            "CBC",
            MinPrice: 10m,
            MaxPrice: 50m,
            PatientLatitude: -17.8,
            PatientLongitude: 31.0,
            Page: 1,
            PageSize: 20);

        var requestBody = LabPartnerElasticsearchSearchSupport.BuildSearchRequestBody(criteria, from: 0, hasGeo: true);

        Assert.Contains("pricing.testType", requestBody, StringComparison.Ordinal);
        Assert.Contains("pricing.price", requestBody, StringComparison.Ordinal);
        Assert.Contains("testTypes", requestBody, StringComparison.Ordinal);
        Assert.Contains("_geo_distance", requestBody, StringComparison.Ordinal);
    }

    [Fact]
    public void ParseSearchResponse_WithMatchingPricing_PopulatesMatchingTestPrice()
    {
        var labPartnerId = Guid.NewGuid();
        var responseBody = $$"""
            {
              "hits": {
                "total": { "value": 1 },
                "hits": [
                  {
                    "sort": [4.1],
                    "_source": {
                      "labPartnerId": "{{labPartnerId}}",
                      "name": "Metro Labs",
                      "address": "456 Lab Avenue",
                      "testTypes": ["CBC", "Lipid Panel"],
                      "pricing": [
                        { "testType": "CBC", "price": 25.50 },
                        { "testType": "Lipid Panel", "price": 40.00 }
                      ]
                    }
                  }
                ]
              }
            }
            """;

        var parsed = LabPartnerElasticsearchSearchSupport.ParseSearchResponse(responseBody, hasGeo: true, "CBC");

        Assert.Single(parsed.Results);
        Assert.Equal(labPartnerId, parsed.Results[0].LabPartnerId);
        Assert.Equal(25.50m, parsed.Results[0].MatchingTestPrice);
        Assert.Equal(4.1, parsed.Results[0].DistanceKilometers);
    }
}
