using FsCheck.Xunit;
using HealthPlatform.Domain.ValueObjects;
using HealthPlatform.Tests.Generators;
using Xunit;

namespace HealthPlatform.Tests.Properties;

/// <summary>
/// Smoke tests ensuring shared generators are well-formed (real properties will reference these).
/// </summary>
public sealed class DomainGeneratorsSmokeTests
{
    [Property(Arbitrary = [typeof(FsCheckArbitraries)], MaxTest = 200)]
    public void GeoPoint_generator_produces_valid_coordinates(GeoPoint point)
    {
        Assert.InRange(point.Latitude, GeoPoint.MinLatitude, GeoPoint.MaxLatitude);
        Assert.InRange(point.Longitude, GeoPoint.MinLongitude, GeoPoint.MaxLongitude);
    }

    [Property(Arbitrary = [typeof(FsCheckArbitraries)], MaxTest = 200)]
    public void Enum_generator_only_emits_defined_values(SampleAppointmentStatus status)
    {
        Assert.Contains(status, Enum.GetValues<SampleAppointmentStatus>());
    }
}
