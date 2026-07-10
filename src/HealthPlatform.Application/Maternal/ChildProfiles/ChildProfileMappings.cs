using HealthPlatform.Domain.Maternal;

namespace HealthPlatform.Application.Maternal.ChildProfiles;

public static class ChildProfileMappings
{
    public static ChildProfileDto ToDto(this ChildProfile profile) =>
        new(
            profile.Id,
            profile.GuardianId,
            profile.FullName,
            profile.DateOfBirth,
            profile.BloodType,
            profile.KnownAllergies,
            profile.HealthRecordId,
            profile.CreatedAtUtc);
}
