using HealthPlatform.Application.Wellness.Summaries;
using HealthPlatform.Domain.Wellness;
using Xunit;

namespace HealthPlatform.Tests.Unit.Wellness;

public sealed class AdherenceSummaryBuilderTests
{
    private static readonly DateTime FromUtc = new(2026, 6, 17, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ToUtc = new(2026, 6, 24, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Build_aggregates_counts_and_rate_across_schedules()
    {
        var schedule = MedicationSchedule.CreateActive(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Amoxicillin",
            [
                new DateTime(2026, 6, 18, 8, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 6, 19, 8, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 6, 20, 8, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 6, 21, 8, 0, 0, DateTimeKind.Utc)
            ]);

        var patientId = schedule.PatientId;
        var events = new List<AdherenceEvent>
        {
            AdherenceEvent.RecordTaken(schedule.Id, patientId,
                new DateTime(2026, 6, 18, 8, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 6, 18, 8, 30, 0, DateTimeKind.Utc)),
            AdherenceEvent.RecordTaken(schedule.Id, patientId,
                new DateTime(2026, 6, 19, 8, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 6, 19, 8, 15, 0, DateTimeKind.Utc)),
            AdherenceEvent.RecordMissed(schedule.Id, patientId,
                new DateTime(2026, 6, 20, 8, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 6, 20, 11, 0, 0, DateTimeKind.Utc))
        };

        var summary = AdherenceSummaryBuilder.Build(
            patientId,
            AdherenceSummaryPeriod.Weekly,
            FromUtc,
            ToUtc,
            [schedule],
            events);

        Assert.Equal("weekly", summary.Period);
        Assert.Equal(4, summary.TotalDoses);
        Assert.Equal(2, summary.TakenDoses);
        Assert.Equal(1, summary.MissedDoses);
        Assert.Equal(0.5d, summary.AdherenceRate);
        Assert.Single(summary.Schedules);
    }

    [Fact]
    public void Build_returns_zero_rate_for_zero_dose_schedule()
    {
        var summary = AdherenceSummaryBuilder.Build(
            Guid.CreateVersion7(),
            AdherenceSummaryPeriod.Monthly,
            FromUtc,
            ToUtc,
            [],
            []);

        Assert.Equal(0, summary.TotalDoses);
        Assert.Equal(0d, summary.AdherenceRate);
        Assert.Empty(summary.Schedules);
    }

    [Fact]
    public void Build_counts_late_doses_as_adherent()
    {
        var schedule = MedicationSchedule.CreateActive(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Ibuprofen",
            [new DateTime(2026, 6, 18, 8, 0, 0, DateTimeKind.Utc)]);

        var lateEvent = AdherenceEvent.RecordTaken(
            schedule.Id,
            schedule.PatientId,
            new DateTime(2026, 6, 18, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 6, 18, 8, 30, 0, DateTimeKind.Utc));

        var summary = AdherenceSummaryBuilder.Build(
            schedule.PatientId,
            AdherenceSummaryPeriod.Weekly,
            FromUtc,
            ToUtc,
            [schedule],
            [lateEvent]);

        Assert.Equal(1, summary.TotalDoses);
        Assert.Equal(1d, summary.AdherenceRate);
    }
}
