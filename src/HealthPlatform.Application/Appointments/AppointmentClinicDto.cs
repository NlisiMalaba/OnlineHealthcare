namespace HealthPlatform.Application.Appointments;

public sealed record AppointmentClinicDto(
    string Address,
    double? Latitude,
    double? Longitude,
    string? GpsNavigationUrl);
