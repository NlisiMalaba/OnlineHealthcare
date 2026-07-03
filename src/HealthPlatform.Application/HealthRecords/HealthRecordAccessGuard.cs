using System.Text.Json;
using HealthPlatform.Application.Audit;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Domain.Audit;

namespace HealthPlatform.Application.HealthRecords;

public sealed class HealthRecordAccessGuard(
    IHealthRecordAccessRepository healthRecordAccessRepository,
    IAuditLogRepository auditLogRepository,
    IAuditContextAccessor auditContext,
    TimeProvider timeProvider) : IHealthRecordAccessGuard
{
    public async Task EnsureDoctorCanReadAsync(Guid healthRecordId, Guid doctorId, CancellationToken ct)
    {
        var activeGrant = await healthRecordAccessRepository.GetActiveGrantAsync(healthRecordId, doctorId, ct);
        if (activeGrant is not null)
        {
            return;
        }

        var timestampUtc = timeProvider.GetUtcNow().UtcDateTime;
        await auditLogRepository.AppendAsync(
            AuditLog.Create(
                doctorId,
                AuditActorType.Doctor,
                AuditActions.HealthRecordAccessDenied,
                "health_record",
                healthRecordId,
                timestampUtc,
                auditContext.IpAddress,
                auditContext.UserAgent,
                JsonSerializer.Serialize(new { doctor_id = doctorId })),
            ct);

        throw new AccessDeniedException(
            "ACCESS_DENIED",
            "Doctor does not have access to this health record.");
    }
}
