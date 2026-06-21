namespace HealthPlatform.API.Requests.Identity;

public sealed class UpdatePharmacyProfileRequest
{
    public string? Name { get; init; }

    public string? Address { get; init; }

    public double? Latitude { get; init; }

    public double? Longitude { get; init; }

    public string? PhoneNumber { get; init; }

    public IFormFile? Logo { get; init; }
}
