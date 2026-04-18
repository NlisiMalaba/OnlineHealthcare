using FsCheck;
using HealthPlatform.Domain.ValueObjects;

namespace HealthPlatform.Tests.Generators;

/// <summary>
/// Shared FsCheck generators for domain primitives (reuse across property tests).
/// </summary>
public static class DomainGenerators
{
    public static Arbitrary<Guid> NonEmptyGuid() =>
        Arb.Default.Guid().Generator.Where(g => g != Guid.Empty).ToArbitrary();

    public static Arbitrary<DateTime> UtcDateTime() =>
        Arb.Default.DateTime()
            .Generator
            .Select(dt => DateTime.SpecifyKind(dt, DateTimeKind.Utc))
            .ToArbitrary();

    /// <summary>
    /// Geo points strictly inside valid WGS-84 bounds (avoids edge floating noise in distance properties).
    /// </summary>
    public static Arbitrary<GeoPoint> WildGeoPoint()
    {
        const double margin = 0.01d;
        var minLatMicro = (int)Math.Ceiling((GeoPoint.MinLatitude + margin) * 1_000_000);
        var maxLatMicro = (int)Math.Floor((GeoPoint.MaxLatitude - margin) * 1_000_000);
        var minLonMicro = (int)Math.Ceiling((GeoPoint.MinLongitude + margin) * 1_000_000);
        var maxLonMicro = (int)Math.Floor((GeoPoint.MaxLongitude - margin) * 1_000_000);

        return Gen.Zip(Gen.Choose(minLatMicro, maxLatMicro), Gen.Choose(minLonMicro, maxLonMicro))
            .Select(z => new GeoPoint(z.Item1 / 1_000_000.0, z.Item2 / 1_000_000.0))
            .ToArbitrary();
    }

    public static Arbitrary<TEnum> EnumValues<TEnum>()
        where TEnum : struct, Enum =>
        Arb.From(Gen.Elements(Enum.GetValues<TEnum>()));
}
