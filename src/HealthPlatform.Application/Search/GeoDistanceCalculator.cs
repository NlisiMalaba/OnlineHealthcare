using HealthPlatform.Domain.ValueObjects;

namespace HealthPlatform.Application.Search;

public static class GeoDistanceCalculator
{
    private const double EarthRadiusKilometers = 6371.0;

    public static double KilometersBetween(GeoPoint origin, GeoPoint destination)
    {
        var originLatitudeRadians = DegreesToRadians(origin.Latitude);
        var destinationLatitudeRadians = DegreesToRadians(destination.Latitude);
        var deltaLatitudeRadians = DegreesToRadians(destination.Latitude - origin.Latitude);
        var deltaLongitudeRadians = DegreesToRadians(destination.Longitude - origin.Longitude);

        var haversine = Math.Sin(deltaLatitudeRadians / 2) * Math.Sin(deltaLatitudeRadians / 2)
            + Math.Cos(originLatitudeRadians) * Math.Cos(destinationLatitudeRadians)
            * Math.Sin(deltaLongitudeRadians / 2) * Math.Sin(deltaLongitudeRadians / 2);

        var centralAngle = 2 * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1 - haversine));
        return EarthRadiusKilometers * centralAngle;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
}
