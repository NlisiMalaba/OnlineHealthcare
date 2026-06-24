using System.Text.Json;
using System.Text.Json.Nodes;
using HealthPlatform.Application.Search;

namespace HealthPlatform.Infrastructure.Search;

public static class PharmacyElasticsearchSearchSupport
{
    public static string BuildSearchRequestBody(PharmacySearchCriteria criteria, int from, bool hasGeo) =>
        PharmacyElasticsearchSearchRequestBuilder.Build(criteria, from, hasGeo);

    public static PharmacySearchPageDto ParseSearchResponse(string? responseBody, bool hasGeo) =>
        PharmacyElasticsearchSearchResponseParser.Parse(responseBody, hasGeo);
}

internal static class PharmacyElasticsearchSearchRequestBuilder
{
    public static string Build(PharmacySearchCriteria criteria, int from, bool hasGeo)
    {
        var filters = new JsonArray
        {
            new JsonObject { ["term"] = new JsonObject { ["isSearchable"] = true } }
        };

        if (criteria.HasStock == true)
        {
            filters.Add(new JsonObject
            {
                ["term"] = new JsonObject { ["hasStock"] = true }
            });
        }

        if (!string.IsNullOrWhiteSpace(criteria.MedicationSku))
        {
            filters.Add(new JsonObject
            {
                ["nested"] = new JsonObject
                {
                    ["path"] = "stockSummary",
                    ["query"] = new JsonObject
                    {
                        ["bool"] = new JsonObject
                        {
                            ["filter"] = new JsonArray
                            {
                                new JsonObject
                                {
                                    ["term"] = new JsonObject
                                    {
                                        ["stockSummary.medicationSku"] = criteria.MedicationSku
                                    }
                                },
                                new JsonObject
                                {
                                    ["range"] = new JsonObject
                                    {
                                        ["stockSummary.quantityOnHand"] = new JsonObject { ["gt"] = 0 }
                                    }
                                }
                            }
                        }
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

    private static JsonArray BuildSort(PharmacySearchCriteria criteria, bool hasGeo)
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

internal static class PharmacyElasticsearchSearchResponseParser
{
    public static PharmacySearchPageDto Parse(string? responseBody, bool hasGeo)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return new PharmacySearchPageDto([], 0);
        }

        using var document = JsonDocument.Parse(responseBody);
        var root = document.RootElement;
        var totalCount = ReadTotalCount(root);

        if (!root.TryGetProperty("hits", out var hitsRoot)
            || !hitsRoot.TryGetProperty("hits", out var hitsArray))
        {
            return new PharmacySearchPageDto([], totalCount);
        }

        var results = new List<PharmacySearchMatchDto>();
        foreach (var hit in hitsArray.EnumerateArray())
        {
            if (!hit.TryGetProperty("_source", out var source))
            {
                continue;
            }

            if (!source.TryGetProperty("pharmacyId", out var pharmacyIdElement)
                || !Guid.TryParse(pharmacyIdElement.GetString(), out var pharmacyId))
            {
                continue;
            }

            double? distanceKm = null;
            if (hasGeo && hit.TryGetProperty("sort", out var sortElement) && sortElement.GetArrayLength() > 0)
            {
                distanceKm = sortElement[0].GetDouble();
            }

            results.Add(new PharmacySearchMatchDto(
                pharmacyId,
                GetString(source, "name"),
                GetString(source, "address"),
                GetBool(source, "hasStock"),
                distanceKm));
        }

        return new PharmacySearchPageDto(results, totalCount);
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

    private static bool GetBool(JsonElement source, string propertyName) =>
        source.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.True;
}
