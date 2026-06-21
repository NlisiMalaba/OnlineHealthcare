namespace HealthPlatform.API.Requests.Identity;

public sealed class UpdateDoctorProfileRequest
{
    public decimal? VirtualFee { get; init; }

    public decimal? PhysicalFee { get; init; }

    public string? Bio { get; init; }

    public string? AvailabilitySlotsJson { get; init; }

    public IFormFile? Photo { get; init; }

    public IFormFile? Credentials { get; init; }
}
