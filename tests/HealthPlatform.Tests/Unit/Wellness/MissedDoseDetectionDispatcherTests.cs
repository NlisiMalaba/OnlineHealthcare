using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Wellness;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Wellness;
using HealthPlatform.Infrastructure.Jobs;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Wellness;

public sealed class MissedDoseDetectionDispatcherTests : IAsyncLifetime
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
    public async Task RecordMissedDoses_creates_missed_event_after_two_hour_grace_period()
    {
        var scheduledAtUtc = new DateTime(2026, 6, 24, 9, 0, 0, DateTimeKind.Utc);
        await SeedScheduleAsync(scheduledAtUtc);

        var dispatcher = _host.GetRequiredService<IMissedDoseDetectionDispatcher>();
        var recorded = await dispatcher.RecordMissedDosesAsync(CancellationToken.None);

        Assert.Equal(1, recorded);

        var adherenceEvent = await _host.DbContext.AdherenceEvents.SingleAsync();
        Assert.Equal(AdherenceEventStatus.Missed, adherenceEvent.Status);
        Assert.Equal(scheduledAtUtc, adherenceEvent.ScheduledAtUtc);
        Assert.Null(adherenceEvent.RecordedAtUtc);
    }

    [Fact]
    public async Task RecordMissedDoses_skips_doses_still_within_grace_period()
    {
        var scheduledAtUtc = new DateTime(2026, 6, 24, 10, 30, 0, DateTimeKind.Utc);
        await SeedScheduleAsync(scheduledAtUtc);

        var dispatcher = _host.GetRequiredService<IMissedDoseDetectionDispatcher>();
        var recorded = await dispatcher.RecordMissedDosesAsync(CancellationToken.None);

        Assert.Equal(0, recorded);
        Assert.False(await _host.DbContext.AdherenceEvents.AnyAsync());
    }

    [Fact]
    public async Task RecordMissedDoses_is_idempotent_after_missed_event_recorded()
    {
        var scheduledAtUtc = new DateTime(2026, 6, 24, 9, 0, 0, DateTimeKind.Utc);
        await SeedScheduleAsync(scheduledAtUtc);

        var dispatcher = _host.GetRequiredService<IMissedDoseDetectionDispatcher>();
        var firstRun = await dispatcher.RecordMissedDosesAsync(CancellationToken.None);
        var secondRun = await dispatcher.RecordMissedDosesAsync(CancellationToken.None);

        Assert.Equal(1, firstRun);
        Assert.Equal(0, secondRun);
        Assert.Single(await _host.DbContext.AdherenceEvents.ToListAsync());
    }

    private async Task SeedScheduleAsync(DateTime doseAtUtc)
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Missed Dose Patient",
                null,
                $"missed-dose-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
        var schedule = MedicationSchedule.CreateActive(
            Guid.CreateVersion7(),
            patient.Id,
            "Amoxicillin",
            [doseAtUtc]);

        await _host.GetRequiredService<IMedicationScheduleRepository>().AddAsync(schedule, CancellationToken.None);
    }
}

public sealed class MissedDoseDetectionJobTests
{
    [Fact]
    public async Task RunAsync_records_missed_doses()
    {
        var dispatcher = new Mock<IMissedDoseDetectionDispatcher>();
        dispatcher
            .Setup(d => d.RecordMissedDosesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var job = new MissedDoseDetectionJob(dispatcher.Object, NullLogger<MissedDoseDetectionJob>.Instance);
        await job.RunAsync(CancellationToken.None);

        dispatcher.Verify(d => d.RecordMissedDosesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
