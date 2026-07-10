using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Maternal.AntenatalRecords.ConfigureFetalMonitoringReminders;
using HealthPlatform.Application.Maternal.AntenatalRecords.CreateAntenatalRecord;
using HealthPlatform.Application.Maternal.AntenatalRecords.RecordAntenatalCheckup;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Maternal;

public sealed class RecordAntenatalCheckupCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Record_checkup_persists_entry_and_configures_fetal_monitoring_reminders()
    {
        var (recordId, obstetricDoctor, scheduleEntryId) = await SeedAntenatalRecordAsync();
        _host.CurrentUser.UserId = obstetricDoctor.UserId;

        var result = await _host.Sender.Send(
            new RecordAntenatalCheckupCommand(
                recordId,
                scheduleEntryId,
                20,
                145,
                21.0m,
                3100m,
                120,
                78,
                69.0m,
                "Fetal movement normal.",
                3),
            CancellationToken.None);

        Assert.Equal(recordId, result.AntenatalRecordId);
        Assert.Equal(scheduleEntryId, result.ScheduleEntryId);
        Assert.Equal(145, result.FetalHeartRateBpm);
        Assert.Equal("Fetal movement normal.", result.ClinicalNotes);

        var storedRecord = await _host.DbContext.AntenatalRecords.SingleAsync(r => r.Id == recordId);
        Assert.Single(storedRecord.EntryRefs);
        Assert.Equal(3, storedRecord.FetalMonitoringReminderIntervalDays);
        Assert.NotNull(storedRecord.NextFetalMonitoringReminderAtUtc);

        var storedScheduleEntry = await _host.DbContext.AntenatalCheckupScheduleEntries
            .SingleAsync(entry => entry.Id == scheduleEntryId);
        Assert.NotNull(storedScheduleEntry.CompletedAtUtc);
        Assert.Equal(result.Id, storedScheduleEntry.CheckupEntryRef);
    }

    [Fact]
    public async Task Record_checkup_rejects_non_assigned_obstetric_doctor()
    {
        var (recordId, _, scheduleEntryId) = await SeedAntenatalRecordAsync();
        var outsider = await SeedVerifiedObstetricDoctorAsync("outsider");
        _host.CurrentUser.UserId = outsider.UserId;

        var ex = await Assert.ThrowsAsync<AccessDeniedException>(() => _host.Sender.Send(
            new RecordAntenatalCheckupCommand(
                recordId,
                scheduleEntryId,
                20,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null),
            CancellationToken.None));

        Assert.Equal("OBSTETRIC_DOCTOR_ACCESS_DENIED", ex.Code);
    }

    [Fact]
    public async Task Configure_fetal_monitoring_reminders_updates_record_interval()
    {
        var (recordId, obstetricDoctor, _) = await SeedAntenatalRecordAsync();
        _host.CurrentUser.UserId = obstetricDoctor.UserId;

        var result = await _host.Sender.Send(
            new ConfigureFetalMonitoringRemindersCommand(recordId, 5),
            CancellationToken.None);

        Assert.Equal(recordId, result.Id);

        var storedRecord = await _host.DbContext.AntenatalRecords.SingleAsync(r => r.Id == recordId);
        Assert.Equal(5, storedRecord.FetalMonitoringReminderIntervalDays);
        Assert.NotNull(storedRecord.NextFetalMonitoringReminderAtUtc);
    }

    private async Task<(Guid RecordId, Doctor ObstetricDoctor, Guid ScheduleEntryId)> SeedAntenatalRecordAsync()
    {
        var obstetricDoctor = await SeedVerifiedObstetricDoctorAsync();
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var record = await _host.Sender.Send(
            new CreateAntenatalRecordCommand(
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(120)),
                12,
                obstetricDoctor.Id),
            CancellationToken.None);

        var scheduleEntryId = record.RecommendedCheckups[0].Id;
        return (record.Id, obstetricDoctor, scheduleEntryId);
    }

    private async Task<Doctor> SeedVerifiedObstetricDoctorAsync(string suffix = "checkup")
    {
        var registration = await _host.Sender.Send(
            ObstetricDoctorRegistrationTestData.CreateValidCommand(
                $"checkup-{suffix}-{Guid.NewGuid():N}@example.com"),
            CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);
        return await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
    }

    private async Task<Patient> SeedPatientAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Checkup Patient",
                null,
                $"checkup-patient-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstAsync();
    }
}
