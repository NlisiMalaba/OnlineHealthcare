using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Application.Wellness;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.NextOfKin;
using HealthPlatform.Domain.Wellness;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Wellness;

public sealed class ConsecutiveMissedDoseAlertTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;
    private FakeTimeProvider _clock = null!;

    public async Task InitializeAsync()
    {
        _clock = new FakeTimeProvider(new DateTime(2026, 6, 24, 18, 0, 0, DateTimeKind.Utc));
        _host = new PatientRegistrationTestHost(timeProvider: _clock);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Missed_dose_detection_emits_alert_to_all_next_of_kin_after_three_consecutive_misses()
    {
        var patient = await SeedPatientWithNextOfKinAsync(contactCount: 2);
        await SeedScheduleAsync(
            patient.Id,
            new DateTime(2026, 6, 24, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 6, 24, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 6, 24, 14, 0, 0, DateTimeKind.Utc));

        var dispatcher = _host.GetRequiredService<IMissedDoseDetectionDispatcher>();
        var recorded = await dispatcher.RecordMissedDosesAsync(CancellationToken.None);

        Assert.Equal(3, recorded);
        Assert.Single(_host.ConsecutiveMissedDosesNextOfKinNotifier.Calls);
        Assert.Equal(2, _host.ConsecutiveMissedDosesNextOfKinNotifier.Calls[0].ContactIds.Count);
        Assert.Equal(patient.Id, _host.ConsecutiveMissedDosesNextOfKinNotifier.Calls[0].PatientId);

        Assert.True(await _host.DbContext.ConsecutiveMissedDoseAlerts.AnyAsync());
        Assert.True(await _host.DbContext.DomainEventOutbox
            .AnyAsync(entry => entry.EventType.Contains("ConsecutiveMissedDosesDetectedDomainEvent")));
    }

    [Fact]
    public async Task Missed_dose_detection_does_not_emit_alert_before_third_consecutive_miss()
    {
        var patient = await SeedPatientWithNextOfKinAsync(contactCount: 1);
        await SeedScheduleAsync(
            patient.Id,
            new DateTime(2026, 6, 24, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 6, 24, 12, 0, 0, DateTimeKind.Utc));

        var dispatcher = _host.GetRequiredService<IMissedDoseDetectionDispatcher>();
        var recorded = await dispatcher.RecordMissedDosesAsync(CancellationToken.None);

        Assert.Equal(2, recorded);
        Assert.Empty(_host.ConsecutiveMissedDosesNextOfKinNotifier.Calls);
        Assert.False(await _host.DbContext.ConsecutiveMissedDoseAlerts.AnyAsync());
    }

    [Fact]
    public async Task Consecutive_missed_dose_alert_is_idempotent_for_same_streak()
    {
        var patient = await SeedPatientWithNextOfKinAsync(contactCount: 1);
        await SeedScheduleAsync(
            patient.Id,
            new DateTime(2026, 6, 24, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 6, 24, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 6, 24, 14, 0, 0, DateTimeKind.Utc));

        var dispatcher = _host.GetRequiredService<IMissedDoseDetectionDispatcher>();
        await dispatcher.RecordMissedDosesAsync(CancellationToken.None);
        await dispatcher.RecordMissedDosesAsync(CancellationToken.None);

        Assert.Single(_host.ConsecutiveMissedDosesNextOfKinNotifier.Calls);
        Assert.Single(await _host.DbContext.ConsecutiveMissedDoseAlerts.ToListAsync());
    }

    private async Task<Patient> SeedPatientWithNextOfKinAsync(int contactCount)
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Consecutive Missed Patient",
                null,
                $"consecutive-missed-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
        var repository = _host.GetRequiredService<INextOfKinRepository>();

        for (var index = 0; index < contactCount; index++)
        {
            await repository.AddAsync(
                NextOfKinContact.Create(
                    patient.Id,
                    $"Contact {index + 1}",
                    "Sibling",
                    $"+1555000{index:0000}",
                    $"contact{index}@example.com",
                    false),
                CancellationToken.None);
        }

        return patient;
    }

    private async Task SeedScheduleAsync(Guid patientId, params DateTime[] doseTimes)
    {
        var schedule = MedicationSchedule.CreateActive(
            Guid.CreateVersion7(),
            patientId,
            "Amoxicillin",
            doseTimes);

        await _host.GetRequiredService<IMedicationScheduleRepository>().AddAsync(schedule, CancellationToken.None);
    }
}
