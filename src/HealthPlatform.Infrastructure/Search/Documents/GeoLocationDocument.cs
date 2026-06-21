using HealthPlatform.Domain.ValueObjects;

namespace HealthPlatform.Infrastructure.Search.Documents;

/// <summary>
/// Elasticsearch geo_point representation (lat/lon object).
/// </summary>
public sealed class GeoLocationDocument
{
    public double Lat { get; init; }

    public double Lon { get; init; }

    public static GeoLocationDocument? FromGeoPoint(GeoPoint? point) =>
        point is null
            ? null
            : new GeoLocationDocument { Lat = point.Latitude, Lon = point.Longitude };
}
