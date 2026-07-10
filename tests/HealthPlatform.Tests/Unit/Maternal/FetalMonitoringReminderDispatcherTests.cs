using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Maternal.AntenatalRecords;
using HealthPlatform.Application.Maternal.AntenatalRecords.ConfigureFetalMonitoringReminders;
using HealthPlatform.Application.Maternal.AntenatalRecords.CreateAntenatalRecord;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Infrastructure.Maternal;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HealthPlatform.Tests.Unit.Maternal;

public sealed class FetalMonitoringReminderDispatcherTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;
    private FakeTimeProvider _clock = null!;

    public async Task InitializeAsync()
    {
        _clock = new FakeTimeProvider(new DateTime(2026, 7, 10, 12, 0, 0, DateTimeKind.Utc));
        _host = new PatientRegistrationTestHost(timeProvider: _clock);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task DispatchDueReminders_notifies_patient_at_doctor_configured_interval()
    {
        var recordId = await ConfigureRecordWithDueReminderAsync();

        var dispatcher = new FetalMonitoringReminderDispatcher(
            _clock,
            _host.GetRequiredService<IAntenatalRecordRepository>(),
            _host.GetRequiredService<IPatientRepository>(),
            _host.FetalMonitoringReminderNotifier,
            NullLogger<FetalMonitoringReminderDispatcher>.Instance);

        var dispatched = await dispatcher.DispatchDueRemindersAsync(CancellationToken.None);

        Assert.Equal(1, dispatched);
        Assert.Single(_host.FetalMonitoringReminderNotifier.Calls);
        Assert.Equal(recordId, _host.FetalMonitoringReminderNotifier.Calls[0].AntenatalRecordId);
        Assert.Equal(4, _host.FetalMonitoringReminderNotifier.Calls[0].IntervalDays);
    }

    private async Task<Guid> ConfigureRecordWithDueReminderAsync()
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

        _host.CurrentUser.UserId = obstetricDoctor.UserId;
        await _host.Sender.Send(
            new ConfigureFetalMonitoringRemindersCommand(record.Id, 4),
            CancellationToken.None);

        var storedRecord = await _host.DbContext.AntenatalRecords.SingleAsync(r => r.Id == record.Id);
        _host.DbContext.Entry(storedRecord).Property(nameof(storedRecord.NextFetalMonitoringReminderAtUtc))
            .CurrentValue = _clock.GetUtcNow().UtcDateTime.AddMinutes(-1);
        await _host.DbContext.SaveChangesAsync();
        _host.DbContext.ChangeTracker.Clear();

        return record.Id;
    }

    private async Task<Doctor> SeedVerifiedObstetricDoctorAsync()
    {
        var registration = await _host.Sender.Send(
            ObstetricDoctorRegistrationTestData.CreateValidCommand(
                $"fetal-reminder-{Guid.NewGuid():N}@example.com"),
            CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);
        return await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
    }

    private async Task<Patient> SeedPatientAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Fetal Reminder Patient",
                null,
                $"fetal-reminder-patient-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstAsync();
    }
}
