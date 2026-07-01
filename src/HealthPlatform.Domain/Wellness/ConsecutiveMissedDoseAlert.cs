using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Wellness;

public sealed class ConsecutiveMissedDoseAlert : Entity
{
    private ConsecutiveMissedDoseAlert()
    {
    }

    public Guid PatientId { get; private set; }

    public Guid TriggeringAdherenceEventId { get; private set; }

    public DateTime StreakEndScheduledAtUtc { get; private set; }

    public DateTime TriggeredAtUtc { get; private set; }

    public static ConsecutiveMissedDoseAlert Record(
        Guid patientId,
        Guid triggeringAdherenceEventId,
        DateTime streakEndScheduledAtUtc,
        DateTime triggeredAtUtc)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (triggeringAdherenceEventId == Guid.Empty)
        {
            throw new ArgumentException("Triggering adherence event id is required.", nameof(triggeringAdherenceEventId));
        }

        if (streakEndScheduledAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Streak end time must be UTC.", nameof(streakEndScheduledAtUtc));
        }

        if (triggeredAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Triggered time must be UTC.", nameof(triggeredAtUtc));
        }

        return new ConsecutiveMissedDoseAlert
        {
            Id = Guid.CreateVersion7(),
            PatientId = patientId,
            TriggeringAdherenceEventId = triggeringAdherenceEventId,
            StreakEndScheduledAtUtc = streakEndScheduledAtUtc,
            TriggeredAtUtc = triggeredAtUtc
        };
    }
}
