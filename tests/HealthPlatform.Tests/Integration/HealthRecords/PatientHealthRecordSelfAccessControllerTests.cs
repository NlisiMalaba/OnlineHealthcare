using HealthPlatform.API.Controllers;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.HealthRecords.CreateHealthRecordEntry;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.HealthRecords;

public sealed class PatientHealthRecordSelfAccessControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task GetAsync_returns_patient_visible_health_record()
    {
        var patientRegistration = await RegisterPatientAsync();
        await SeedVisibleEntryAsync(patientRegistration, "Visible consultation note.");

        _host.CurrentUser.UserId = await GetPatientUserIdAsync(patientRegistration.PatientId);
        var controller = new PatientHealthRecordController(_host.Sender);

        var result = await controller.GetAsync(CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var record = Assert.IsType<PatientHealthRecordDto>(ok.Value);

        Assert.Equal(patientRegistration.HealthRecordId, record.HealthRecordId);
        Assert.Equal(patientRegistration.PatientId, record.PatientId);
        Assert.Single(record.Entries);
        Assert.Equal("Visible consultation note.", record.Entries[0].Content.ConsultationNote!.Notes);
    }

    [Fact]
    public async Task ExportPdfAsync_returns_signed_download_url()
    {
        var patientRegistration = await RegisterPatientAsync();
        await SeedVisibleEntryAsync(patientRegistration, "PDF export note.");

        _host.CurrentUser.UserId = await GetPatientUserIdAsync(patientRegistration.PatientId);
        var controller = new PatientHealthRecordController(_host.Sender);

        var result = await controller.ExportPdfAsync(CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var export = Assert.IsType<HealthRecordPdfExportDto>(ok.Value);

        Assert.Equal(patientRegistration.HealthRecordId, export.HealthRecordId);
        Assert.StartsWith("file:///", export.DownloadUrl, StringComparison.OrdinalIgnoreCase);
    }

    private async Task SeedVisibleEntryAsync(
        PatientRegistrationResponseDto patientRegistration,
        string notes)
    {
        var doctorUserId = await RegisterVerifiedDoctorAsync();
        _host.CurrentUser.UserId = doctorUserId;

        await _host.Sender.Send(
            new CreateHealthRecordEntryCommand(
                patientRegistration.HealthRecordId,
                HealthRecordEntryType.ConsultationNote,
                new HealthRecordEntryContentPayload(
                    ConsultationNote: new ConsultationNoteContent(notes, null)),
                IsVisibleToPatient: true),
            CancellationToken.None);
    }

    private async Task<PatientRegistrationResponseDto> RegisterPatientAsync() =>
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Self Access Patient",
                null,
                $"self-access-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

    private async Task<Guid> GetPatientUserIdAsync(Guid patientId)
    {
        var patient = await _host.DbContext.Patients.SingleAsync(p => p.Id == patientId);
        return patient.UserId;
    }

    private async Task<Guid> RegisterVerifiedDoctorAsync()
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        await _host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);

        var doctor = await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
        return doctor.UserId;
    }
}
