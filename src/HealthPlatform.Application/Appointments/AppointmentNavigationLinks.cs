using HealthPlatform.Domain.ValueObjects;

namespace HealthPlatform.Application.Appointments;

public static class AppointmentNavigationLinks
{
    public static string? CreateGpsNavigationUrl(GeoPoint? location, string? address)
    {
        if (location is not null)
        {
            return $"https://www.google.com/maps/dir/?api=1&destination={location.Latitude:F6},{location.Longitude:F6}";
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            return null;
        }

        return $"https://www.google.com/maps/dir/?api=1&destination={Uri.EscapeDataString(address.Trim())}";
    }
}
