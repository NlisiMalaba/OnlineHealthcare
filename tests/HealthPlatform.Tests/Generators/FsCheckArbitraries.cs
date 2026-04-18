using FsCheck;
using HealthPlatform.Domain.ValueObjects;

namespace HealthPlatform.Tests.Generators;

public enum SampleAppointmentStatus
{
    Requested = 0,
    Confirmed = 1,
    Cancelled = 2
}

/// <summary>
/// Static arbitrary providers discovered by FsCheck.Xunit via <c>[Property(Arbitrary = ...)]</c>.
/// </summary>
public static class FsCheckArbitraries
{
    public static Arbitrary<GeoPoint> GeoPoint() => DomainGenerators.WildGeoPoint();

    public static Arbitrary<SampleAppointmentStatus> SampleAppointmentStatus() =>
        DomainGenerators.EnumValues<SampleAppointmentStatus>();
}
