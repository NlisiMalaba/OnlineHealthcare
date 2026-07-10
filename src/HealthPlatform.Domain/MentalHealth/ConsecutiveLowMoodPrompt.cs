using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.MentalHealth;

public sealed class ConsecutiveLowMoodPrompt : Entity
{
    private ConsecutiveLowMoodPrompt()
    {
        TriggeringMoodLogId = string.Empty;
    }

    public Guid PatientId { get; private set; }

    public string TriggeringMoodLogId { get; private set; }

    public DateTime StreakEndLoggedAtUtc { get; private set; }

    public DateTime TriggeredAtUtc { get; private set; }

    public static ConsecutiveLowMoodPrompt Record(
        Guid patientId,
        string triggeringMoodLogId,
        DateTime streakEndLoggedAtUtc,
        DateTime triggeredAtUtc)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (string.IsNullOrWhiteSpace(triggeringMoodLogId))
        {
            throw new ArgumentException("Triggering mood log id is required.", nameof(triggeringMoodLogId));
        }

        if (streakEndLoggedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Streak end time must be UTC.", nameof(streakEndLoggedAtUtc));
        }

        if (triggeredAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Triggered time must be UTC.", nameof(triggeredAtUtc));
        }

        return new ConsecutiveLowMoodPrompt
        {
            Id = Guid.CreateVersion7(),
            PatientId = patientId,
            TriggeringMoodLogId = triggeringMoodLogId.Trim(),
            StreakEndLoggedAtUtc = streakEndLoggedAtUtc,
            TriggeredAtUtc = triggeredAtUtc
        };
    }
}
