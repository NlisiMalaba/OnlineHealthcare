using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Wellness;

public sealed class MedicationDoseReminder : Entity
{
    private MedicationDoseReminder()
    {
    }

    public Guid ScheduleId { get; private set; }

    public Guid PatientId { get; private set; }

    public DateTime ScheduledAtUtc { get; private set; }

    public DateTime SentAtUtc { get; private set; }

    public static MedicationDoseReminder RecordSent(
        Guid scheduleId,
        Guid patientId,
        DateTime scheduledAtUtc,
        DateTime sentAtUtc)
    {
        if (scheduleId == Guid.Empty)
        {
            throw new ArgumentException("Schedule id is required.", nameof(scheduleId));
        }

        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (scheduledAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Scheduled dose time must be UTC.", nameof(scheduledAtUtc));
        }

        if (sentAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Sent time must be UTC.", nameof(sentAtUtc));
        }

        if (sentAtUtc < scheduledAtUtc)
        {
            throw new ArgumentException("Sent time cannot be before the scheduled dose time.", nameof(sentAtUtc));
        }

        return new MedicationDoseReminder
        {
            Id = Guid.CreateVersion7(),
            ScheduleId = scheduleId,
            PatientId = patientId,
            ScheduledAtUtc = scheduledAtUtc,
            SentAtUtc = sentAtUtc
        };
    }
}
