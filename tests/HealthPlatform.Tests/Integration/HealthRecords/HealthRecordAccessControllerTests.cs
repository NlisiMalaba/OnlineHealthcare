using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.HealthRecords;
using HealthPlatform.Application.Audit;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.HealthRecords.GrantHealthRecordAccess;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Domain.Audit;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.HealthRecords;

public sealed class HealthRecordAccessControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task GrantAsync_allows_doctor_to_list_health_record_entries()
    {
        var patientRegistration = await RegisterPatientAsync();
        var doctorRegistration = await RegisterVerifiedDoctorAsync();
        var patientUserId = await GetPatientUserIdAsync(patientRegistration.PatientId);

        _host.CurrentUser.UserId = patientUserId;
        var accessController = new PatientHealthRecordAccessController(_host.Sender);
        await accessController.GrantAsync(
            new GrantHealthRecordAccessRequest { DoctorId = doctorRegistration.DoctorId },
            CancellationToken.None);

        _host.CurrentUser.UserId = doctorRegistration.DoctorUserId;
        var entriesController = new HealthRecordEntriesController(_host.Sender);
        var listResult = await entriesController.ListAsync(patientRegistration.HealthRecordId, CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(listResult.Result);
        Assert.IsAssignableFrom<IReadOnlyList<HealthRecordEntryDto>>(ok.Value);
    }

    [Fact]
    public async Task ListAsync_without_grant_throws_access_denied_and_writes_audit_log()
    {
        var patientRegistration = await RegisterPatientAsync();
        var doctorRegistration = await RegisterVerifiedDoctorAsync();

        _host.CurrentUser.UserId = doctorRegistration.DoctorUserId;
        var entriesController = new HealthRecordEntriesController(_host.Sender);

        await Assert.ThrowsAsync<AccessDeniedException>(async () =>
            _ = await entriesController.ListAsync(patientRegistration.HealthRecordId, CancellationToken.None));

        var auditLog = await _host.DbContext.AuditLogs.SingleAsync();
        Assert.Equal(AuditActions.HealthRecordAccessDenied, auditLog.Action);
        Assert.Equal(doctorRegistration.DoctorId, auditLog.ActorId);
        Assert.Equal(patientRegistration.HealthRecordId, auditLog.ResourceId);
    }

    [Fact]
    public async Task RevokeAsync_blocks_subsequent_doctor_reads()
    {
        var patientRegistration = await RegisterPatientAsync();
        var doctorRegistration = await RegisterVerifiedDoctorAsync();
        var patientUserId = await GetPatientUserIdAsync(patientRegistration.PatientId);

        _host.CurrentUser.UserId = patientUserId;
        var accessController = new PatientHealthRecordAccessController(_host.Sender);
        await accessController.GrantAsync(
            new GrantHealthRecordAccessRequest { DoctorId = doctorRegistration.DoctorId },
            CancellationToken.None);

        await accessController.RevokeAsync(doctorRegistration.DoctorId, CancellationToken.None);

        _host.CurrentUser.UserId = doctorRegistration.DoctorUserId;
        var entriesController = new HealthRecordEntriesController(_host.Sender);
        await Assert.ThrowsAsync<AccessDeniedException>(async () =>
            _ = await entriesController.ListAsync(patientRegistration.HealthRecordId, CancellationToken.None));
    }

    private async Task<PatientRegistrationResponseDto> RegisterPatientAsync() =>
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Access Control Patient",
                null,
                $"access-control-{Guid.NewGuid():N}@example.com",
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
