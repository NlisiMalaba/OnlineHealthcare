using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Identity;

public sealed class PatientRegistrationWorkflowTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task RegisterWithPhone_CreatesPatientAndLinkedHealthRecord()
    {
        var response = await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Phone,
                "Jane Doe",
                "+263771234567",
                null,
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.SingleAsync(p => p.Id == response.PatientId);
        var healthRecord = await _host.DbContext.HealthRecords.SingleAsync(r => r.Id == response.HealthRecordId);

        Assert.Equal(patient.Id, healthRecord.PatientId);
        Assert.Equal(PatientAuthProvider.Phone, patient.AuthProvider);
    }

    [Fact]
    public async Task RegisterWithDuplicatePhone_ReturnsIdentityConflict()
    {
        var command = new RegisterPatientCommand(
            PatientAuthProvider.Phone,
            "Jane Doe",
            "+263779999999",
            null,
            PatientRegistrationTestHost.ValidPassword,
            null);

        await _host.Sender.Send(command, CancellationToken.None);

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            _host.Sender.Send(command, CancellationToken.None));

        Assert.Equal(IdentityErrorCodes.IdentityConflict, ex.Code);
    }

    [Fact]
    public async Task RegisterWithGoogle_CreatesPatientAndHealthRecord()
    {
        var idToken = PatientRegistrationTestHost.CreateSocialIdToken(
            "google-subject-123",
            "google.user@example.com",
            "Google User");

        var response = await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Google,
                "Google User",
                null,
                null,
                null,
                idToken),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.SingleAsync(p => p.Id == response.PatientId);
        Assert.Equal(PatientAuthProvider.Google, patient.AuthProvider);
        Assert.Equal("google.user@example.com", patient.Email);
        Assert.True(await _host.DbContext.HealthRecords.AnyAsync(r => r.PatientId == patient.Id));
    }
}
