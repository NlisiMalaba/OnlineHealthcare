namespace HealthPlatform.Domain.ValueObjects;

/// <summary>
/// WGS-84 latitude/longitude pair (e.g. clinic locations, Elasticsearch geo_point).
/// </summary>
public sealed class GeoPoint : ValueObject
{
    public const double MinLatitude = -90.0;
    public const double MaxLatitude = 90.0;
    public const double MinLongitude = -180.0;
    public const double MaxLongitude = 180.0;

    public GeoPoint(double latitude, double longitude)
    {
        if (latitude is < MinLatitude or > MaxLatitude)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude));
        }

        if (longitude is < MinLongitude or > MaxLongitude)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude));
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    public double Latitude { get; }
    public double Longitude { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }

    public override string ToString() => $"{Latitude:F6},{Longitude:F6}";
}
