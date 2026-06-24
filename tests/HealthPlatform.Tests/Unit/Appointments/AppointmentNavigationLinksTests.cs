using HealthPlatform.Application.Appointments;
using HealthPlatform.Domain.ValueObjects;
using Xunit;

namespace HealthPlatform.Tests.Unit.Appointments;

public sealed class AppointmentNavigationLinksTests
{
    [Fact]
    public void CreateGpsNavigationUrl_uses_coordinates_when_available()
    {
        var location = new GeoPoint(-17.8252, 31.0335);

        var url = AppointmentNavigationLinks.CreateGpsNavigationUrl(location, "12 Samora Machel Ave, Harare");

        Assert.Equal(
            "https://www.google.com/maps/dir/?api=1&destination=-17.825200,31.033500",
            url);
    }

    [Fact]
    public void CreateGpsNavigationUrl_falls_back_to_address_when_coordinates_missing()
    {
        var url = AppointmentNavigationLinks.CreateGpsNavigationUrl(
            null,
            "12 Samora Machel Ave, Harare");

        Assert.Equal(
            "https://www.google.com/maps/dir/?api=1&destination=12%20Samora%20Machel%20Ave%2C%20Harare",
            url);
    }

    [Fact]
    public void CreateGpsNavigationUrl_returns_null_when_no_location_data()
    {
        Assert.Null(AppointmentNavigationLinks.CreateGpsNavigationUrl(null, null));
        Assert.Null(AppointmentNavigationLinks.CreateGpsNavigationUrl(null, "   "));
    }
}
