using HealthPlatform.Domain.Wellness;
using Xunit;

namespace HealthPlatform.Tests.Unit.Wellness;

public sealed class AdherenceEventTests
{
    [Fact]
    public void RecordTaken_sets_status_and_recorded_timestamp()
    {
        var scheduledAtUtc = new DateTime(2026, 6, 24, 8, 0, 0, DateTimeKind.Utc);
        var recordedAtUtc = scheduledAtUtc.AddMinutes(15);

        var adherenceEvent = AdherenceEvent.RecordTaken(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            scheduledAtUtc,
            recordedAtUtc);

        Assert.Equal(AdherenceEventStatus.Taken, adherenceEvent.Status);
        Assert.Equal(recordedAtUtc, adherenceEvent.RecordedAtUtc);
        Assert.Equal(scheduledAtUtc, adherenceEvent.ScheduledAtUtc);
    }

    [Fact]
    public void RecordMissed_sets_status_without_recorded_timestamp()
    {
        var scheduledAtUtc = new DateTime(2026, 6, 24, 8, 0, 0, DateTimeKind.Utc);
        var detectedAtUtc = scheduledAtUtc.AddHours(2);

        var adherenceEvent = AdherenceEvent.RecordMissed(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            scheduledAtUtc,
            detectedAtUtc);

        Assert.Equal(AdherenceEventStatus.Missed, adherenceEvent.Status);
        Assert.Null(adherenceEvent.RecordedAtUtc);
    }
}
