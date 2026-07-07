using HealthPlatform.Application.Audit;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Domain.Audit;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.HealthRecords;

public sealed class HealthRecordAccessAuditServiceTests
{
    [Fact]
    public async Task LogGrantAsync_persists_audit_entry_with_actor_action_and_timestamp()
    {
        await using var host = new PatientRegistrationTestHost();
        var auditService = host.GetRequiredService<IHealthRecordAccessAuditService>();
        var patientId = Guid.CreateVersion7();
        var healthRecordId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();

        await auditService.LogGrantAsync(
            patientId,
            healthRecordId,
            doctorId,
            HealthRecordAccessType.Full,
            CancellationToken.None);

        var log = await host.DbContext.AuditLogs.SingleAsync();
        Assert.Equal(patientId, log.ActorId);
        Assert.Equal(AuditActorType.Patient, log.ActorType);
        Assert.Equal(AuditActions.HealthRecordAccessGranted, log.Action);
        Assert.Equal(healthRecordId, log.ResourceId);
        Assert.Equal("health_record", log.ResourceType);
        Assert.Contains(doctorId.ToString(), log.MetadataJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LogDoctorAccessAttemptAsync_persists_denied_and_allowed_actions()
    {
        await using var host = new PatientRegistrationTestHost();
        var auditService = host.GetRequiredService<IHealthRecordAccessAuditService>();
        var doctorId = Guid.CreateVersion7();
        var healthRecordId = Guid.CreateVersion7();

        await auditService.LogDoctorAccessAttemptAsync(
            doctorId,
            healthRecordId,
            HealthRecordAccessOperations.ListEntries,
            allowed: false,
            CancellationToken.None);

        await auditService.LogDoctorAccessAttemptAsync(
            doctorId,
            healthRecordId,
            HealthRecordAccessOperations.ListEntries,
            allowed: true,
            CancellationToken.None);

        var logs = await host.DbContext.AuditLogs
            .AsNoTracking()
            .OrderBy(log => log.TimestampUtc)
            .ToListAsync();

        Assert.Equal(2, logs.Count);
        Assert.Equal(AuditActions.HealthRecordAccessDenied, logs[0].Action);
        Assert.Equal(AuditActions.HealthRecordAccessed, logs[1].Action);
        Assert.All(logs, log => Assert.Equal(doctorId, log.ActorId));
    }

    [Fact]
    public async Task LogRevokeAsync_persists_audit_entry_with_actor_and_resource()
    {
        await using var host = new PatientRegistrationTestHost();
        var auditService = host.GetRequiredService<IHealthRecordAccessAuditService>();
        var patientId = Guid.CreateVersion7();
        var healthRecordId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();

        await auditService.LogRevokeAsync(patientId, healthRecordId, doctorId, CancellationToken.None);

        var log = await host.DbContext.AuditLogs.SingleAsync();
        Assert.Equal(patientId, log.ActorId);
        Assert.Equal(AuditActorType.Patient, log.ActorType);
        Assert.Equal(AuditActions.HealthRecordAccessRevoked, log.Action);
        Assert.Equal(healthRecordId, log.ResourceId);
        Assert.Contains(doctorId.ToString(), log.MetadataJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LogPatientAccessAsync_persists_accessed_action_with_operation_metadata()
    {
        await using var host = new PatientRegistrationTestHost();
        var auditService = host.GetRequiredService<IHealthRecordAccessAuditService>();
        var patientId = Guid.CreateVersion7();
        var healthRecordId = Guid.CreateVersion7();

        await auditService.LogPatientAccessAsync(
            patientId,
            healthRecordId,
            HealthRecordAccessOperations.ExportPdf,
            CancellationToken.None);

        var log = await host.DbContext.AuditLogs.SingleAsync();
        Assert.Equal(AuditActions.HealthRecordAccessed, log.Action);
        Assert.Contains(HealthRecordAccessOperations.ExportPdf, log.MetadataJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LogPatientAccessAttemptAsync_persists_denied_patient_entry_access()
    {
        await using var host = new PatientRegistrationTestHost();
        var auditService = host.GetRequiredService<IHealthRecordAccessAuditService>();
        var patientId = Guid.CreateVersion7();
        var healthRecordId = Guid.CreateVersion7();

        await auditService.LogPatientAccessAttemptAsync(
            patientId,
            healthRecordId,
            HealthRecordAccessOperations.GetPatientEntry,
            allowed: false,
            CancellationToken.None);

        var log = await host.DbContext.AuditLogs.SingleAsync();
        Assert.Equal(AuditActions.HealthRecordAccessDenied, log.Action);
        Assert.Equal(patientId, log.ActorId);
        Assert.Contains(HealthRecordAccessOperations.GetPatientEntry, log.MetadataJson, StringComparison.Ordinal);
    }
}
