namespace HealthPlatform.API.Requests.Identity;

public sealed class RegisterDoctorRequest
{
    public string FullName { get; init; } = string.Empty;

    public string LicenseNumber { get; init; } = string.Empty;

    public string Specialty { get; init; } = string.Empty;

    public int YearsOfExperience { get; init; }

    public string ClinicAddress { get; init; } = string.Empty;

    public double? ClinicLatitude { get; init; }

    public double? ClinicLongitude { get; init; }

    public decimal VirtualFee { get; init; }

    public decimal PhysicalFee { get; init; }

    public string? Bio { get; init; }

    public string Email { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string? AvailabilitySlotsJson { get; init; }

    public IFormFile? Photo { get; init; }

    public IFormFile? Credentials { get; init; }
}
