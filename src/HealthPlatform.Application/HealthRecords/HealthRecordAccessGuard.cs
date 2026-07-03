using HealthPlatform.Application.Exceptions;

namespace HealthPlatform.Application.HealthRecords;

public sealed class HealthRecordAccessGuard(
    IHealthRecordAccessRepository healthRecordAccessRepository,
    IHealthRecordAccessAuditService healthRecordAccessAuditService) : IHealthRecordAccessGuard
{
    public async Task EnsureDoctorCanReadAsync(
        Guid healthRecordId,
        Guid doctorId,
        string accessOperation,
        CancellationToken ct)
    {
        var activeGrant = await healthRecordAccessRepository.GetActiveGrantAsync(healthRecordId, doctorId, ct);
        var allowed = activeGrant is not null;

        await healthRecordAccessAuditService.LogDoctorAccessAttemptAsync(
            doctorId,
            healthRecordId,
            accessOperation,
            allowed,
            ct);

        if (!allowed)
        {
            throw new AccessDeniedException(
                "ACCESS_DENIED",
                "Doctor does not have access to this health record.");
        }
    }
}
