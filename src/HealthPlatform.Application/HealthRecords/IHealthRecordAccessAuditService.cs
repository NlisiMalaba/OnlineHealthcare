using HealthPlatform.Domain.HealthRecords;

namespace HealthPlatform.Application.HealthRecords;

public interface IHealthRecordAccessAuditService
{
    Task LogGrantAsync(
        Guid patientId,
        Guid healthRecordId,
        Guid doctorId,
        HealthRecordAccessType accessType,
        CancellationToken ct);

    Task LogRevokeAsync(
        Guid patientId,
        Guid healthRecordId,
        Guid doctorId,
        CancellationToken ct);

    Task LogDoctorAccessAttemptAsync(
        Guid doctorId,
        Guid healthRecordId,
        string operation,
        bool allowed,
        CancellationToken ct);

    Task LogPatientAccessAsync(
        Guid patientId,
        Guid healthRecordId,
        string operation,
        CancellationToken ct);

    Task LogPatientAccessAttemptAsync(
        Guid patientId,
        Guid healthRecordId,
        string operation,
        bool allowed,
        CancellationToken ct);
}
