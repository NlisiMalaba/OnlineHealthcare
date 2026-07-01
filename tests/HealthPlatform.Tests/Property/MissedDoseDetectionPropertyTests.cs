using FsCheck.Xunit;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Wellness;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Wellness;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Properties;

public sealed class MissedDoseDetectionPropertyTests
{
    private static readonly DateTime ReferenceNowUtc = new(2026, 6, 24, 12, 0, 0, DateTimeKind.Utc);

    // Feature: online-healthcare-platform, Property 19: Missed Dose Detection
    [Property(Arbitrary = [typeof(WellnessArbitraries)], MaxTest = 100)]
    public bool Unconfirmed_dose_adherence_status_matches_grace_period(MissedDoseDetectionCase input) =>
        RunMissedDoseDetectionInvariantAsync(input).GetAwaiter().GetResult();

    private static async Task<bool> RunMissedDoseDetectionInvariantAsync(MissedDoseDetectionCase input)
    {
        var scheduledAtUtc = input.Expectation switch
        {
            MissedDoseDetectionExpectation.RecordsMissed =>
                ReferenceNowUtc
                    .Subtract(WellnessPolicies.MissedDoseGracePeriod)
                    .AddMinutes(-input.OffsetMinutes),
            MissedDoseDetectionExpectation.WithinGracePeriod =>
                ReferenceNowUtc
                    .Subtract(WellnessPolicies.MissedDoseGracePeriod)
                    .AddMinutes(input.OffsetMinutes),
            _ => throw new ArgumentOutOfRangeException(nameof(input))
        };

        var clock = new FakeTimeProvider(ReferenceNowUtc);
        await using var host = new PatientRegistrationTestHost(timeProvider: clock);

        var schedule = await SeedScheduleAsync(host, scheduledAtUtc);
        var dispatcher = host.GetRequiredService<IMissedDoseDetectionDispatcher>();
        var recorded = await dispatcher.RecordMissedDosesAsync(CancellationToken.None);

        var adherenceEvent = await host.DbContext.AdherenceEvents
            .AsNoTracking()
            .SingleOrDefaultAsync(adherenceEvent => adherenceEvent.ScheduleId == schedule.Id
                && adherenceEvent.ScheduledAtUtc == scheduledAtUtc);

        return input.Expectation switch
        {
            MissedDoseDetectionExpectation.RecordsMissed =>
                recorded == 1
                && adherenceEvent is not null
                && adherenceEvent.Status == AdherenceEventStatus.Missed
                && adherenceEvent.PatientId == schedule.PatientId
                && adherenceEvent.RecordedAtUtc is null
                && WellnessPolicies.IsMissed(scheduledAtUtc, ReferenceNowUtc),
            MissedDoseDetectionExpectation.WithinGracePeriod =>
                recorded == 0
                && adherenceEvent is null
                && !WellnessPolicies.IsMissed(scheduledAtUtc, ReferenceNowUtc),
            _ => false
        };
    }

    private static async Task<MedicationSchedule> SeedScheduleAsync(
        PatientRegistrationTestHost host,
        DateTime scheduledAtUtc)
    {
        await host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Missed Dose Property Patient",
                null,
                $"missed-dose-property-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
        var schedule = MedicationSchedule.CreateActive(
            Guid.CreateVersion7(),
            patient.Id,
            "Amoxicillin",
            [scheduledAtUtc]);

        await host.GetRequiredService<IMedicationScheduleRepository>().AddAsync(schedule, CancellationToken.None);
        return schedule;
    }
}
