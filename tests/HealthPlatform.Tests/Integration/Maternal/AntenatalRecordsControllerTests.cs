using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Maternal;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Maternal;

public sealed class AntenatalRecordsControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Create_returns_created_antenatal_record()
    {
        var obstetricDoctor = await SeedVerifiedObstetricDoctorAsync();
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;
        var dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(120));
        var controller = new AntenatalRecordsController(_host.Sender);

        var result = await controller.CreateAsync(
            new CreateAntenatalRecordRequest(dueDate, 12, obstetricDoctor.Id),
            CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result.Result);
        var dto = Assert.IsType<HealthPlatform.Application.Maternal.AntenatalRecords.AntenatalRecordDto>(created.Value);
        Assert.Equal(patient.Id, dto.PatientId);
        Assert.NotEmpty(dto.RecommendedCheckups);
        Assert.Equal($"/api/v1/maternal/antenatal-records/{dto.Id}", created.Location);
    }

    private async Task<Doctor> SeedVerifiedObstetricDoctorAsync()
    {
        var registration = await _host.Sender.Send(
            ObstetricDoctorRegistrationTestData.CreateValidCommand(
                $"controller-obstetric-{Guid.NewGuid():N}@example.com"),
            CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);
        return await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
    }

    private async Task<Patient> SeedPatientAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Controller Patient",
                null,
                $"controller-patient-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstAsync();
    }
}
