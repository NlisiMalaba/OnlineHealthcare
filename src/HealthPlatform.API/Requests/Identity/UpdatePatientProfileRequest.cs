using HealthPlatform.Domain.Identity;

namespace HealthPlatform.API.Requests.Identity;

public sealed class UpdatePatientProfileRequest
{
    public string? FullName { get; init; }

    public DateOnly? DateOfBirth { get; init; }

    public BloodType? BloodType { get; init; }

    public List<string>? KnownAllergies { get; init; }

    public List<string>? ChronicConditions { get; init; }

    public IFormFile? Photo { get; init; }
}
