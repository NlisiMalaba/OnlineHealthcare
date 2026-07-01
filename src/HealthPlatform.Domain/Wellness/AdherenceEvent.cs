using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Wellness;

public sealed class AdherenceEvent : Entity
{
    private AdherenceEvent()
    {
    }

    public Guid ScheduleId { get; private set; }

    public Guid PatientId { get; private set; }

    public DateTime ScheduledAtUtc { get; private set; }

    public DateTime? RecordedAtUtc { get; private set; }

    public AdherenceEventStatus Status { get; private set; }

    public static AdherenceEvent RecordTaken(
        Guid scheduleId,
        Guid patientId,
        DateTime scheduledAtUtc,
        DateTime recordedAtUtc)
    {
        ValidateScheduleContext(scheduleId, patientId, scheduledAtUtc);
        ValidateRecordedAt(scheduledAtUtc, recordedAtUtc);

        return new AdherenceEvent
        {
            Id = Guid.CreateVersion7(),
            ScheduleId = scheduleId,
            PatientId = patientId,
            ScheduledAtUtc = scheduledAtUtc,
            RecordedAtUtc = recordedAtUtc,
            Status = AdherenceEventStatus.Taken
        };
    }

    public static AdherenceEvent RecordMissed(
        Guid scheduleId,
        Guid patientId,
        DateTime scheduledAtUtc,
        DateTime detectedAtUtc)
    {
        ValidateScheduleContext(scheduleId, patientId, scheduledAtUtc);

        if (detectedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Detection time must be UTC.", nameof(detectedAtUtc));
        }

        if (detectedAtUtc < scheduledAtUtc)
        {
            throw new ArgumentException("Detection time cannot be before the scheduled dose time.", nameof(detectedAtUtc));
        }

        return new AdherenceEvent
        {
            Id = Guid.CreateVersion7(),
            ScheduleId = scheduleId,
            PatientId = patientId,
            ScheduledAtUtc = scheduledAtUtc,
            RecordedAtUtc = null,
            Status = AdherenceEventStatus.Missed
        };
    }

    private static void ValidateScheduleContext(Guid scheduleId, Guid patientId, DateTime scheduledAtUtc)
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
    }

    private static void ValidateRecordedAt(DateTime scheduledAtUtc, DateTime recordedAtUtc)
    {
        if (recordedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Recorded time must be UTC.", nameof(recordedAtUtc));
        }

        if (recordedAtUtc < scheduledAtUtc)
        {
            throw new ArgumentException("Recorded time cannot be before the scheduled dose time.", nameof(recordedAtUtc));
        }
    }
}
