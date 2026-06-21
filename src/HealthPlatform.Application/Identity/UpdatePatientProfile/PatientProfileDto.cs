using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Identity.UpdatePatientProfile;

public sealed record PatientProfileDto(
    Guid PatientId,
    string FullName,
    DateOnly? DateOfBirth,
    BloodType? BloodType,
    IReadOnlyList<string> KnownAllergies,
    IReadOnlyList<string> ChronicConditions,
    string? ProfilePhotoUrl,
    DateTime UpdatedAtUtc);
