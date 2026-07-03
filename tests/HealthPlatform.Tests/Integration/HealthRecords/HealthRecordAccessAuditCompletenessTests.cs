using HealthPlatform.API.Controllers;
using HealthPlatform.Application.Audit;
using HealthPlatform.Application.HealthRecords.GrantHealthRecordAccess;
using HealthPlatform.Application.HealthRecords.RevokeHealthRecordAccess;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Domain.Audit;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.HealthRecords;

public sealed class HealthRecordAccessAuditCompletenessTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Grant_revoke_and_access_attempts_each_write_audit_log_entries()
    {
        var patientRegistration = await RegisterPatientAsync();
        var doctorRegistration = await RegisterVerifiedDoctorAsync();
        var patientUserId = await GetPatientUserIdAsync(patientRegistration.PatientId);

        _host.CurrentUser.UserId = patientUserId;
        var accessController = new PatientHealthRecordAccessController(_host.Sender);
        await accessController.GrantAsync(
            new API.Requests.HealthRecords.GrantHealthRecordAccessRequest
            {
                DoctorId = doctorRegistration.DoctorId
            },
            CancellationToken.None);

        _host.CurrentUser.UserId = doctorRegistration.DoctorUserId;
        var entriesController = new HealthRecordEntriesController(_host.Sender);
        await entriesController.ListAsync(patientRegistration.HealthRecordId, CancellationToken.None);

        _host.CurrentUser.UserId = patientUserId;
        await accessController.RevokeAsync(doctorRegistration.DoctorId, CancellationToken.None);

        var logs = await _host.DbContext.AuditLogs
            .AsNoTracking()
            .OrderBy(log => log.TimestampUtc)
            .ToListAsync();

        Assert.Contains(logs, log => log.Action == AuditActions.HealthRecordAccessGranted);
        Assert.Contains(logs, log => log.Action == AuditActions.HealthRecordAccessed);
        Assert.Contains(logs, log => log.Action == AuditActions.HealthRecordAccessRevoked);
        Assert.All(logs, log => Assert.NotEqual(default, log.TimestampUtc));
        Assert.All(logs, log => Assert.False(string.IsNullOrWhiteSpace(log.Action)));
    }

    private async Task<PatientRegistrationResponseDto> RegisterPatientAsync() =>
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Audit Completeness Patient",
                null,
                $"audit-complete-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

    private async Task<(Guid DoctorId, Guid DoctorUserId)> RegisterVerifiedDoctorAsync()
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        await _host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);

        var doctor = await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
        return (doctor.Id, doctor.UserId);
    }

    private async Task<Guid> GetPatientUserIdAsync(Guid patientId)
    {
        var patient = await _host.DbContext.Patients.SingleAsync(p => p.Id == patientId);
        return patient.UserId;
    }
}
