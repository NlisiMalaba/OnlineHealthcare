using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Domain.HealthRecords;

namespace HealthPlatform.Application.HealthRecords;

public static class HealthRecordAccessMappings
{
    public static HealthRecordAccessDto ToDto(this HealthRecordAccess access, string doctorFullName) =>
        new(
            access.Id,
            access.HealthRecordId,
            access.GrantedToDoctorId,
            doctorFullName,
            access.AccessType,
            access.Sections,
            access.GrantedAtUtc,
            access.RevokedAtUtc,
            access.IsActive);
}
