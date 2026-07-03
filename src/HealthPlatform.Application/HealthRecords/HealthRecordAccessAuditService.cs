using System.Text.Json;
using HealthPlatform.Application.Audit;
using HealthPlatform.Domain.Audit;
using HealthPlatform.Domain.HealthRecords;

namespace HealthPlatform.Application.HealthRecords;

public sealed class HealthRecordAccessAuditService(
    IAuditLogRepository auditLogRepository,
    IAuditContextAccessor auditContext,
    TimeProvider timeProvider) : IHealthRecordAccessAuditService
{
    public Task LogGrantAsync(
        Guid patientId,
        Guid healthRecordId,
        Guid doctorId,
        HealthRecordAccessType accessType,
        CancellationToken ct) =>
        AppendAsync(
            patientId,
            AuditActorType.Patient,
            AuditActions.HealthRecordAccessGranted,
            healthRecordId,
            JsonSerializer.Serialize(new
            {
                doctor_id = doctorId,
                access_type = accessType.ToString()
            }),
            ct);

    public Task LogRevokeAsync(
        Guid patientId,
        Guid healthRecordId,
        Guid doctorId,
        CancellationToken ct) =>
        AppendAsync(
            patientId,
            AuditActorType.Patient,
            AuditActions.HealthRecordAccessRevoked,
            healthRecordId,
            JsonSerializer.Serialize(new { doctor_id = doctorId }),
            ct);

    public Task LogDoctorAccessAttemptAsync(
        Guid doctorId,
        Guid healthRecordId,
        string operation,
        bool allowed,
        CancellationToken ct) =>
        AppendAsync(
            doctorId,
            AuditActorType.Doctor,
            allowed ? AuditActions.HealthRecordAccessed : AuditActions.HealthRecordAccessDenied,
            healthRecordId,
            JsonSerializer.Serialize(new
            {
                doctor_id = doctorId,
                operation,
                allowed
            }),
            ct);

    public Task LogPatientAccessAsync(
        Guid patientId,
        Guid healthRecordId,
        string operation,
        CancellationToken ct) =>
        LogPatientAccessAttemptAsync(patientId, healthRecordId, operation, allowed: true, ct);

    public Task LogPatientAccessAttemptAsync(
        Guid patientId,
        Guid healthRecordId,
        string operation,
        bool allowed,
        CancellationToken ct) =>
        AppendAsync(
            patientId,
            AuditActorType.Patient,
            allowed ? AuditActions.HealthRecordAccessed : AuditActions.HealthRecordAccessDenied,
            healthRecordId,
            JsonSerializer.Serialize(new
            {
                operation,
                allowed
            }),
            ct);

    private Task AppendAsync(
        Guid actorId,
        AuditActorType actorType,
        string action,
        Guid healthRecordId,
        string metadataJson,
        CancellationToken ct)
    {
        var timestampUtc = timeProvider.GetUtcNow().UtcDateTime;
        return auditLogRepository.AppendAsync(
            AuditLog.Create(
                actorId,
                actorType,
                action,
                "health_record",
                healthRecordId,
                timestampUtc,
                auditContext.IpAddress,
                auditContext.UserAgent,
                metadataJson),
            ct);
    }
}
