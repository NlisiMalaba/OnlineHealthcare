namespace HealthPlatform.API.Requests.Identity;

public sealed class RegisterPharmacyRequest
{
    public string Name { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public double? Latitude { get; init; }

    public double? Longitude { get; init; }

    public string Email { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public IFormFile? Logo { get; init; }
}
