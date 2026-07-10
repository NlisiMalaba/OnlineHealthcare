using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Maternal.AntenatalRecords;
using HealthPlatform.Application.Maternal.AntenatalRecords.CreateAntenatalRecord;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Infrastructure.Maternal;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HealthPlatform.Tests.Unit.Maternal;

public sealed class AntenatalCheckupReminderDispatcherTests : IAsyncLifetime
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
    public async Task DispatchDueReminders_sends_high_frequency_reminder_when_due_date_within_four_weeks()
    {
        var obstetricDoctor = await SeedVerifiedObstetricDoctorAsync();
        var patient = await SeedPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;
        var dueDate = new DateOnly(2026, 8, 1);

        var record = await _host.Sender.Send(
            new CreateAntenatalRecordCommand(dueDate, 36, obstetricDoctor.Id),
            CancellationToken.None);

        await BackdateNextReminderAsync(record.Id, _clock.GetUtcNow().UtcDateTime.AddMinutes(-1));
        var dispatcher = CreateDispatcher();

        var dispatched = await dispatcher.DispatchDueRemindersAsync(CancellationToken.None);

        Assert.Equal(1, dispatched);
        Assert.Single(_host.AntenatalCheckupReminderNotifier.Calls);
        Assert.True(_host.AntenatalCheckupReminderNotifier.Calls[0].HighFrequency);
        Assert.Equal(patient.UserId, _host.AntenatalCheckupReminderNotifier.Calls[0].PatientUserId);
        Assert.Equal(obstetricDoctor.UserId, _host.AntenatalCheckupReminderNotifier.Calls[0].ObstetricDoctorUserId);
    }

    private AntenatalCheckupReminderDispatcher CreateDispatcher() =>
        new(
            _clock,
            _host.GetRequiredService<IAntenatalRecordRepository>(),
            _host.GetRequiredService<IPatientRepository>(),
            _host.GetRequiredService<IDoctorRepository>(),
            _host.AntenatalCheckupReminderNotifier,
            NullLogger<AntenatalCheckupReminderDispatcher>.Instance);

    private async Task BackdateNextReminderAsync(Guid antenatalRecordId, DateTime nextReminderAtUtc)
    {
        var record = await _host.DbContext.AntenatalRecords.SingleAsync(r => r.Id == antenatalRecordId);
        _host.DbContext.Entry(record).Property(nameof(record.NextReminderAtUtc)).CurrentValue = nextReminderAtUtc;
        await _host.DbContext.SaveChangesAsync();
        _host.DbContext.ChangeTracker.Clear();
    }

    private async Task<Doctor> SeedVerifiedObstetricDoctorAsync()
    {
        var registration = await _host.Sender.Send(
            ObstetricDoctorRegistrationTestData.CreateValidCommand(
                $"reminder-obstetric-{Guid.NewGuid():N}@example.com"),
            CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(registration.DoctorId), CancellationToken.None);
        return await _host.DbContext.Doctors.SingleAsync(d => d.Id == registration.DoctorId);
    }

    private async Task<Patient> SeedPatientAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Reminder Patient",
                null,
                $"reminder-patient-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstAsync();
    }
}
