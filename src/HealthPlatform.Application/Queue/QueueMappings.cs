using HealthPlatform.Domain.Queue;

namespace HealthPlatform.Application.Queue;

public static class QueueMappings
{
    public static QueueEntryDto ToDto(this QueueEntry entry) =>
        new(
            entry.Id,
            entry.AppointmentId,
            entry.PatientId,
            entry.DoctorId,
            entry.PatientName,
            entry.AppointmentScheduledAtUtc,
            entry.QueuePosition,
            entry.EstimatedWaitMinutes,
            ToApiStatus(entry.ArrivalStatus),
            entry.JoinedAtUtc);

    private static string ToApiStatus(QueueArrivalStatus status) =>
        status switch
        {
            QueueArrivalStatus.NotArrived => "not_arrived",
            QueueArrivalStatus.Arrived => "arrived",
            QueueArrivalStatus.Called => "called",
            QueueArrivalStatus.Seen => "seen",
            QueueArrivalStatus.Absent => "absent",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
}
