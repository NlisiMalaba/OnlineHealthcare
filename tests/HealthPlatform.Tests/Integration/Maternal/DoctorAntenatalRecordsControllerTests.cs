using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Maternal;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Maternal.AntenatalRecords.CreateAntenatalRecord;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Maternal;

public sealed class DoctorAntenatalRecordsControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task RecordCheckup_returns_created_checkup_entry()
    {
        var obstetricDoctor = await SeedVerifiedObstetricDoctorAsync();
        var record = await SeedAntenatalRecordAsync(obstetricDoctor);
        _host.CurrentUser.UserId = obstetricDoctor.UserId;
        var controller = new DoctorAntenatalRecordsController(_host.Sender);

        var result = await controller.RecordCheckupAsync(
            record.Id,
            new RecordAntenatalCheckupRequest(
                record.RecommendedCheckups[0].Id,
                20,
                142,
                20.5m,
                3000m,
                118,
                74,
                67.5m,
                "Healthy fetal heartbeat.",
                2),
            CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result.Result);
        var entry = Assert.IsType<HealthPlatform.Application.Maternal.AntenatalRecords.AntenatalCheckupEntryDto>(
            created.Value);
        Assert.Equal(142, entry.FetalHeartRateBpm);
        Assert.Equal(2, (await _host.DbContext.AntenatalRecords.SingleAsync(r => r.Id == record.Id))
            .FetalMonitoringReminderIntervalDays);
    }

    private async Task<HealthPlatform.Application.Maternal.AntenatalRecords.AntenatalRecordDto> SeedAntenatalRecordAsync(
        Doctor obstetricDoctor)
    {
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;

        return await _host.Sender.Send(
            new CreateAntenatalRecordCommand(
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(120)),
                12,
                obstetricDoctor.Id),
            CancellationToken.None);
    }

    private async Task<Doctor> SeedVerifiedObstetricDoctorAsync()
    {
        var registration = await _host.Sender.Send(
            ObstetricDoctorRegistrationTestData.CreateValidCommand(
                $"doctor-controller-{Guid.NewGuid():N}@example.com"),
            CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);
        return await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
    }

    private async Task<Patient> SeedPatientAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Doctor Controller Patient",
                null,
                $"doctor-controller-patient-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstAsync();
    }
}
