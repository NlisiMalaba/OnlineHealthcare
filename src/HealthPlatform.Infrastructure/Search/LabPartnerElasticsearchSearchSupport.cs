using System.Text.Json;
using System.Text.Json.Nodes;
using HealthPlatform.Application.Search;

namespace HealthPlatform.Infrastructure.Search;

public static class LabPartnerElasticsearchSearchSupport
{
    public static string BuildSearchRequestBody(LabPartnerSearchCriteria criteria, int from, bool hasGeo) =>
        LabPartnerElasticsearchSearchRequestBuilder.Build(criteria, from, hasGeo);

    public static LabPartnerSearchPageDto ParseSearchResponse(
        string? responseBody,
        bool hasGeo,
        string? testTypeFilter) =>
        LabPartnerElasticsearchSearchResponseParser.Parse(responseBody, hasGeo, testTypeFilter);
}

internal static class LabPartnerElasticsearchSearchRequestBuilder
{
    public static string Build(LabPartnerSearchCriteria criteria, int from, bool hasGeo)
    {
        var filters = new JsonArray
        {
            new JsonObject { ["term"] = new JsonObject { ["isSearchable"] = true } }
        };

        if (!string.IsNullOrWhiteSpace(criteria.TestType))
        {
            filters.Add(new JsonObject
            {
                ["term"] = new JsonObject { ["testTypes"] = criteria.TestType }
            });

            var nestedFilters = new JsonArray
            {
                new JsonObject
                {
                    ["term"] = new JsonObject { ["pricing.testType"] = criteria.TestType }
                }
            };

            if (criteria.MinPrice.HasValue)
            {
                nestedFilters.Add(new JsonObject
                {
                    ["range"] = new JsonObject
                    {
                        ["pricing.price"] = new JsonObject { ["gte"] = criteria.MinPrice.Value }
                    }
                });
            }

            if (criteria.MaxPrice.HasValue)
            {
                nestedFilters.Add(new JsonObject
                {
                    ["range"] = new JsonObject
                    {
                        ["pricing.price"] = new JsonObject { ["lte"] = criteria.MaxPrice.Value }
                    }
                });
            }

            filters.Add(new JsonObject
            {
                ["nested"] = new JsonObject
                {
                    ["path"] = "pricing",
                    ["query"] = new JsonObject
                    {
                        ["bool"] = new JsonObject { ["filter"] = nestedFilters }
                    }
                }
            });
        }
        else if (criteria.MinPrice.HasValue || criteria.MaxPrice.HasValue)
        {
            var nestedFilters = new JsonArray();
            if (criteria.MinPrice.HasValue)
            {
                nestedFilters.Add(new JsonObject
                {
                    ["range"] = new JsonObject
                    {
                        ["pricing.price"] = new JsonObject { ["gte"] = criteria.MinPrice.Value }
                    }
                });
            }

            if (criteria.MaxPrice.HasValue)
            {
                nestedFilters.Add(new JsonObject
                {
                    ["range"] = new JsonObject
                    {
                        ["pricing.price"] = new JsonObject { ["lte"] = criteria.MaxPrice.Value }
                    }
                });
            }

            filters.Add(new JsonObject
            {
                ["nested"] = new JsonObject
                {
                    ["path"] = "pricing",
                    ["query"] = new JsonObject
                    {
                        ["bool"] = new JsonObject { ["filter"] = nestedFilters }
                    }
                }
            });
        }

        var sort = BuildSort(criteria, hasGeo);

        var root = new JsonObject
        {
            ["from"] = from,
            ["size"] = criteria.PageSize,
            ["track_total_hits"] = true,
            ["query"] = new JsonObject
            {
                ["bool"] = new JsonObject { ["filter"] = filters }
            },
            ["sort"] = sort
        };

        return root.ToJsonString();
    }

    private static JsonArray BuildSort(LabPartnerSearchCriteria criteria, bool hasGeo)
    {
        if (hasGeo)
        {
            return
            [
                new JsonObject
                {
                    ["_geo_distance"] = new JsonObject
                    {
                        ["location"] = new JsonObject
                        {
                            ["lat"] = criteria.PatientLatitude!.Value,
                            ["lon"] = criteria.PatientLongitude!.Value
                        },
                        ["order"] = "asc",
                        ["unit"] = "km"
                    }
                }
            ];
        }

        return
        [
            new JsonObject
            {
                ["name.keyword"] = new JsonObject { ["order"] = "asc" }
            }
        ];
    }
}

internal static class LabPartnerElasticsearchSearchResponseParser
{
    public static LabPartnerSearchPageDto Parse(string? responseBody, bool hasGeo, string? testTypeFilter)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return new LabPartnerSearchPageDto([], 0);
        }

        using var document = JsonDocument.Parse(responseBody);
        var root = document.RootElement;
        var totalCount = ReadTotalCount(root);

        if (!root.TryGetProperty("hits", out var hitsRoot)
            || !hitsRoot.TryGetProperty("hits", out var hitsArray))
        {
            return new LabPartnerSearchPageDto([], totalCount);
        }

        var results = new List<LabPartnerSearchMatchDto>();
        foreach (var hit in hitsArray.EnumerateArray())
        {
            if (!hit.TryGetProperty("_source", out var source))
            {
                continue;
            }

            if (!source.TryGetProperty("labPartnerId", out var labPartnerIdElement)
                || !Guid.TryParse(labPartnerIdElement.GetString(), out var labPartnerId))
            {
                continue;
            }

            double? distanceKm = null;
            if (hasGeo && hit.TryGetProperty("sort", out var sortElement) && sortElement.GetArrayLength() > 0)
            {
                distanceKm = sortElement[0].GetDouble();
            }

            var testTypes = ReadStringArray(source, "testTypes");
            var matchingPrice = ResolveMatchingTestPrice(source, testTypeFilter);

            results.Add(new LabPartnerSearchMatchDto(
                labPartnerId,
                GetString(source, "name"),
                GetString(source, "address"),
                testTypes,
                matchingPrice,
                distanceKm));
        }

        return new LabPartnerSearchPageDto(results, totalCount);
    }

    private static decimal? ResolveMatchingTestPrice(JsonElement source, string? testTypeFilter)
    {
        if (string.IsNullOrWhiteSpace(testTypeFilter)
            || !source.TryGetProperty("pricing", out var pricingElement)
            || pricingElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var pricingEntry in pricingElement.EnumerateArray())
        {
            if (!pricingEntry.TryGetProperty("testType", out var testTypeElement))
            {
                continue;
            }

            if (!string.Equals(testTypeElement.GetString(), testTypeFilter, StringComparison.Ordinal))
            {
                continue;
            }

            if (pricingEntry.TryGetProperty("price", out var priceElement)
                && priceElement.TryGetDecimal(out var price))
            {
                return price;
            }
        }

        return null;
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement source, string propertyName)
    {
        if (!source.TryGetProperty(propertyName, out var arrayElement)
            || arrayElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return arrayElement.EnumerateArray()
            .Select(element => element.GetString())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToList();
    }

    private static long ReadTotalCount(JsonElement root)
    {
        if (!root.TryGetProperty("hits", out var hitsRoot)
            || !hitsRoot.TryGetProperty("total", out var totalElement))
        {
            return 0;
        }

        return totalElement.ValueKind switch
        {
            JsonValueKind.Number => totalElement.GetInt64(),
            JsonValueKind.Object when totalElement.TryGetProperty("value", out var valueElement) =>
                valueElement.GetInt64(),
            _ => 0
        };
    }

    private static string GetString(JsonElement source, string propertyName) =>
        source.TryGetProperty(propertyName, out var value) ? value.GetString() ?? string.Empty : string.Empty;
}
