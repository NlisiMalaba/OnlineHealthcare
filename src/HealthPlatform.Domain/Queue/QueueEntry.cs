using HealthPlatform.Domain.Common;
using HealthPlatform.Domain.Queue.Events;

namespace HealthPlatform.Domain.Queue;

public sealed class QueueEntry : Entity
{
    private QueueEntry()
    {
        PatientName = string.Empty;
    }

    public Guid AppointmentId { get; private set; }

    public Guid PatientId { get; private set; }

    public Guid DoctorId { get; private set; }

    public string PatientName { get; private set; }

    public DateTime AppointmentScheduledAtUtc { get; private set; }

    public int QueuePosition { get; private set; }

    public int EstimatedWaitMinutes { get; private set; }

    public QueueArrivalStatus ArrivalStatus { get; private set; }

    public int? ActualWaitMinutes { get; private set; }

    public DateTime JoinedAtUtc { get; private set; }

    public DateTime? PositionTwoNotifiedAtUtc { get; private set; }

    public static QueueEntry Create(
        Guid appointmentId,
        Guid patientId,
        Guid doctorId,
        string patientName,
        DateTime appointmentScheduledAtUtc,
        int queuePosition,
        int estimatedWaitMinutes,
        DateTime joinedAtUtc)
    {
        if (appointmentId == Guid.Empty)
        {
            throw new ArgumentException("Appointment id is required.", nameof(appointmentId));
        }

        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (doctorId == Guid.Empty)
        {
            throw new ArgumentException("Doctor id is required.", nameof(doctorId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(patientName);

        if (appointmentScheduledAtUtc == default)
        {
            throw new ArgumentException("Appointment scheduled time is required.", nameof(appointmentScheduledAtUtc));
        }

        if (appointmentScheduledAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Appointment scheduled time must be UTC.", nameof(appointmentScheduledAtUtc));
        }

        if (queuePosition <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(queuePosition));
        }

        if (estimatedWaitMinutes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(estimatedWaitMinutes));
        }

        if (joinedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Joined time must be UTC.", nameof(joinedAtUtc));
        }

        var entry = new QueueEntry
        {
            Id = Guid.CreateVersion7(),
            AppointmentId = appointmentId,
            PatientId = patientId,
            DoctorId = doctorId,
            PatientName = patientName.Trim(),
            AppointmentScheduledAtUtc = appointmentScheduledAtUtc,
            QueuePosition = queuePosition,
            EstimatedWaitMinutes = estimatedWaitMinutes,
            ArrivalStatus = QueueArrivalStatus.NotArrived,
            JoinedAtUtc = joinedAtUtc,
            CreatedAtUtc = joinedAtUtc,
            UpdatedAtUtc = joinedAtUtc
        };

        entry.RaiseDomainEvent(new QueueEntryCreatedDomainEvent(
            entry.Id,
            entry.AppointmentId,
            entry.PatientId,
            entry.DoctorId,
            entry.QueuePosition,
            entry.EstimatedWaitMinutes,
            entry.JoinedAtUtc));

        return entry;
    }

    public bool MarkPositionTwoNotified(DateTime notifiedAtUtc)
    {
        if (notifiedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Notification time must be UTC.", nameof(notifiedAtUtc));
        }

        if (QueuePosition != 2 || PositionTwoNotifiedAtUtc.HasValue)
        {
            return false;
        }

        PositionTwoNotifiedAtUtc = notifiedAtUtc;
        Touch();
        return true;
    }
}
