namespace HealthPlatform.Application.Maternal.ChildProfiles;

public sealed record ChildProfileDto(
    Guid Id,
    Guid GuardianId,
    string FullName,
    DateOnly DateOfBirth,
    string? BloodType,
    IReadOnlyList<string> KnownAllergies,
    Guid HealthRecordId,
    DateTime CreatedAtUtc);
