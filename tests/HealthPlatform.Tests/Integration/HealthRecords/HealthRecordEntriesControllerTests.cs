using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.HealthRecords;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Infrastructure.MongoDb;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.HealthRecords;

public sealed class HealthRecordEntriesControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task CreateAsync_persists_entry_and_patient_can_list_visible_entries()
    {
        var patientRegistration = await RegisterPatientAsync();
        var doctorUserId = await RegisterVerifiedDoctorAsync();

        _host.CurrentUser.UserId = doctorUserId;
        var doctorController = new HealthRecordEntriesController(_host.Sender);
        var createdResult = await doctorController.CreateAsync(
            patientRegistration.HealthRecordId,
            new CreateHealthRecordEntryRequest
            {
                EntryType = "consultation_note",
                IsVisibleToPatient = true,
                ConsultationNote = new ConsultationNoteContentRequest
                {
                    Notes = "Follow-up consultation completed."
                }
            },
            CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(createdResult.Result);
        var entry = Assert.IsType<HealthRecordEntryDto>(created.Value);
        Assert.Equal(HealthRecordEntryType.ConsultationNote, entry.EntryType);

        _host.CurrentUser.UserId = await GetPatientUserIdAsync(patientRegistration.PatientId);
        var patientController = new PatientHealthRecordController(_host.Sender);
        var listResult = await patientController.ListEntriesAsync(CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(listResult.Result);
        var entries = Assert.IsAssignableFrom<IReadOnlyList<HealthRecordEntryDto>>(ok.Value);
        Assert.Single(entries);
        Assert.Equal("Follow-up consultation completed.", entries[0].Content.ConsultationNote!.Notes);
    }

    [Fact]
    public async Task Patient_list_excludes_entries_not_visible_to_patient()
    {
        var patientRegistration = await RegisterPatientAsync();
        var doctorUserId = await RegisterVerifiedDoctorAsync();

        _host.CurrentUser.UserId = doctorUserId;
        var doctorController = new HealthRecordEntriesController(_host.Sender);
        await doctorController.CreateAsync(
            patientRegistration.HealthRecordId,
            new CreateHealthRecordEntryRequest
            {
                EntryType = "diagnosis",
                IsVisibleToPatient = false,
                Diagnosis = new DiagnosisContentRequest
                {
                    DiagnosisCodes = ["R51"],
                    Description = "Internal working diagnosis"
                }
            },
            CancellationToken.None);

        _host.CurrentUser.UserId = await GetPatientUserIdAsync(patientRegistration.PatientId);
        var patientController = new PatientHealthRecordController(_host.Sender);
        var listResult = await patientController.ListEntriesAsync(CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(listResult.Result);
        var entries = Assert.IsAssignableFrom<IReadOnlyList<HealthRecordEntryDto>>(ok.Value);
        Assert.Empty(entries);
    }

    private async Task<PatientRegistrationResponseDto> RegisterPatientAsync() =>
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Health Record Patient",
                null,
                $"health-record-{Guid.NewGuid():N}@example.com",
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
