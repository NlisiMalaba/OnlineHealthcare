using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Wellness;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Wellness;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HealthPlatform.Tests.Unit.Wellness;

public sealed class MedicationDoseReminderDispatcherTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;
    private FakeTimeProvider _clock = null!;

    public async Task InitializeAsync()
    {
        _clock = new FakeTimeProvider(new DateTime(2026, 6, 24, 12, 0, 0, DateTimeKind.Utc));
        _host = new PatientRegistrationTestHost(timeProvider: _clock);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task DispatchDueReminders_notifies_patient_when_dose_time_is_reached()
    {
        var schedule = await SeedScheduleAsync(
            new DateTime(2026, 6, 24, 11, 58, 0, DateTimeKind.Utc));

        var dispatcher = CreateDispatcher();
        var dispatched = await dispatcher.DispatchDueRemindersAsync(CancellationToken.None);

        Assert.Equal(1, dispatched);
        Assert.Single(_host.MedicationDoseReminderNotifier.Calls);
        Assert.Equal(schedule.Id, _host.MedicationDoseReminderNotifier.Calls[0].ScheduleId);

        var reminder = await _host.DbContext.MedicationDoseReminders
            .SingleAsync(r => r.ScheduleId == schedule.Id);
        Assert.Equal(schedule.PatientId, reminder.PatientId);
    }

    [Fact]
    public async Task DispatchDueReminders_skips_doses_outside_lookback_window()
    {
        await SeedScheduleAsync(new DateTime(2026, 6, 24, 11, 0, 0, DateTimeKind.Utc));

        var dispatcher = CreateDispatcher();
        var dispatched = await dispatcher.DispatchDueRemindersAsync(CancellationToken.None);

        Assert.Equal(0, dispatched);
        Assert.Empty(_host.MedicationDoseReminderNotifier.Calls);
    }

    [Fact]
    public async Task DispatchDueReminders_is_idempotent_after_reminder_sent()
    {
        await SeedScheduleAsync(new DateTime(2026, 6, 24, 11, 58, 0, DateTimeKind.Utc));

        var dispatcher = CreateDispatcher();
        var firstRun = await dispatcher.DispatchDueRemindersAsync(CancellationToken.None);
        var secondRun = await dispatcher.DispatchDueRemindersAsync(CancellationToken.None);

        Assert.Equal(1, firstRun);
        Assert.Equal(0, secondRun);
        Assert.Single(_host.MedicationDoseReminderNotifier.Calls);
    }

    private MedicationDoseReminderDispatcher CreateDispatcher() =>
        new(
            _clock,
            _host.GetRequiredService<IMedicationDoseReminderRepository>(),
            _host.GetRequiredService<IPatientRepository>(),
            _host.MedicationDoseReminderNotifier,
            NullLogger<MedicationDoseReminderDispatcher>.Instance);

    private async Task<MedicationSchedule> SeedScheduleAsync(DateTime dueDoseAtUtc)
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Dose Reminder Patient",
                null,
                $"dose-reminder-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
        var schedule = MedicationSchedule.CreateActive(
            Guid.CreateVersion7(),
            patient.Id,
            "Amoxicillin",
            [dueDoseAtUtc]);

        await _host.GetRequiredService<IMedicationScheduleRepository>().AddAsync(schedule, CancellationToken.None);
        return schedule;
    }
}
