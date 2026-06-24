using System.Text.Json;
using System.Text.Json.Nodes;
using HealthPlatform.Application.Search;
using HealthPlatform.Infrastructure.Search.Documents;

namespace HealthPlatform.Infrastructure.Search;

public static class DoctorElasticsearchSearchSupport
{
    public static string BuildSearchRequestBody(DoctorSearchCriteria criteria, int from, bool hasGeo) =>
        DoctorElasticsearchSearchRequestBuilder.Build(criteria, from, hasGeo);

    public static DoctorSearchPageDto ParseSearchResponse(string? responseBody, bool hasGeo) =>
        DoctorElasticsearchSearchResponseParser.Parse(responseBody, hasGeo);

    public static string BuildSimulatedSearchResponse(
        IReadOnlyList<(DoctorSearchDocument Document, double DistanceKilometers)> hits) =>
        DoctorElasticsearchSearchResponseParser.BuildSimulatedResponse(hits);
}

internal static class DoctorElasticsearchSearchRequestBuilder
{
    public static string Build(DoctorSearchCriteria criteria, int from, bool hasGeo)
    {
        var filters = new JsonArray
        {
            new JsonObject { ["term"] = new JsonObject { ["isSearchable"] = true } }
        };

        if (!string.IsNullOrWhiteSpace(criteria.Specialty))
        {
            filters.Add(new JsonObject
            {
                ["term"] = new JsonObject { ["specialty"] = criteria.Specialty }
            });
        }

        if (criteria.MinRating.HasValue)
        {
            filters.Add(new JsonObject
            {
                ["range"] = new JsonObject
                {
                    ["averageRating"] = new JsonObject { ["gte"] = criteria.MinRating.Value }
                }
            });
        }

        if (criteria.MinFee.HasValue)
        {
            filters.Add(new JsonObject
            {
                ["range"] = new JsonObject
                {
                    ["maxFee"] = new JsonObject { ["gte"] = criteria.MinFee.Value }
                }
            });
        }

        if (criteria.MaxFee.HasValue)
        {
            filters.Add(new JsonObject
            {
                ["range"] = new JsonObject
                {
                    ["minFee"] = new JsonObject { ["lte"] = criteria.MaxFee.Value }
                }
            });
        }

        if (criteria.HasAvailability == true)
        {
            filters.Add(new JsonObject
            {
                ["term"] = new JsonObject { ["hasAvailability"] = true }
            });
        }

        JsonArray sort;
        if (hasGeo)
        {
            sort =
            [
                new JsonObject
                {
                    ["_geo_distance"] = new JsonObject
                    {
                        ["clinicLocation"] = new JsonObject
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
        else
        {
            sort =
            [
                new JsonObject
                {
                    ["averageRating"] = new JsonObject { ["order"] = "desc" }
                },
                new JsonObject
                {
                    ["name.keyword"] = new JsonObject { ["order"] = "asc" }
                }
            ];
        }

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
}

internal static class DoctorElasticsearchSearchResponseParser
{
    public static DoctorSearchPageDto Parse(string? responseBody, bool hasGeo)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return new DoctorSearchPageDto([], 0);
        }

        using var document = JsonDocument.Parse(responseBody);
        var root = document.RootElement;

        var totalCount = 0L;
        if (root.TryGetProperty("hits", out var hitsRoot)
            && hitsRoot.TryGetProperty("total", out var totalElement))
        {
            totalCount = totalElement.ValueKind switch
            {
                JsonValueKind.Number => totalElement.GetInt64(),
                JsonValueKind.Object when totalElement.TryGetProperty("value", out var valueElement) =>
                    valueElement.GetInt64(),
                _ => 0
            };
        }

        if (!root.TryGetProperty("hits", out hitsRoot)
            || !hitsRoot.TryGetProperty("hits", out var hitsArray))
        {
            return new DoctorSearchPageDto([], totalCount);
        }

        var results = new List<DoctorSearchMatchDto>();
        foreach (var hit in hitsArray.EnumerateArray())
        {
            if (!hit.TryGetProperty("_source", out var source))
            {
                continue;
            }

            var doctorId = source.TryGetProperty("doctorId", out var doctorIdElement)
                && Guid.TryParse(doctorIdElement.GetString(), out var parsedDoctorId)
                ? parsedDoctorId
                : Guid.Empty;

            if (doctorId == Guid.Empty)
            {
                continue;
            }

            double? distanceKm = null;
            if (hasGeo && hit.TryGetProperty("sort", out var sortElement) && sortElement.GetArrayLength() > 0)
            {
                distanceKm = sortElement[0].GetDouble();
            }

            results.Add(new DoctorSearchMatchDto(
                doctorId,
                GetString(source, "name"),
                GetString(source, "specialty"),
                GetDecimal(source, "averageRating"),
                GetInt(source, "totalReviews"),
                GetDecimal(source, "virtualFee"),
                GetDecimal(source, "physicalFee"),
                distanceKm));
        }

        return new DoctorSearchPageDto(results, totalCount);
    }

    public static string BuildSimulatedResponse(
        IReadOnlyList<(DoctorSearchDocument Document, double DistanceKilometers)> hits)
    {
        var hitArray = new JsonArray();
        foreach (var (document, distanceKilometers) in hits)
        {
            hitArray.Add(new JsonObject
            {
                ["_source"] = new JsonObject
                {
                    ["doctorId"] = document.DoctorId,
                    ["name"] = document.Name,
                    ["specialty"] = document.Specialty,
                    ["averageRating"] = document.AverageRating,
                    ["totalReviews"] = document.TotalReviews,
                    ["virtualFee"] = document.VirtualFee,
                    ["physicalFee"] = document.PhysicalFee
                },
                ["sort"] = new JsonArray { distanceKilometers }
            });
        }

        var root = new JsonObject
        {
            ["hits"] = new JsonObject
            {
                ["total"] = new JsonObject { ["value"] = hits.Count },
                ["hits"] = hitArray
            }
        };

        return root.ToJsonString();
    }

    private static string GetString(JsonElement source, string propertyName) =>
        source.TryGetProperty(propertyName, out var value) ? value.GetString() ?? string.Empty : string.Empty;

    private static decimal GetDecimal(JsonElement source, string propertyName) =>
        source.TryGetProperty(propertyName, out var value) && value.TryGetDecimal(out var decimalValue)
            ? decimalValue
            : 0m;

    private static int GetInt(JsonElement source, string propertyName) =>
        source.TryGetProperty(propertyName, out var value) && value.TryGetInt32(out var intValue)
            ? intValue
            : 0;
}
