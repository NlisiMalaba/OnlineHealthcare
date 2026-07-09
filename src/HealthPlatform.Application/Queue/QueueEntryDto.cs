namespace HealthPlatform.Application.Queue;

public sealed record QueueEntryDto(
    Guid Id,
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    string PatientName,
    DateTime AppointmentScheduledAtUtc,
    int QueuePosition,
    int EstimatedWaitMinutes,
    string ArrivalStatus,
    DateTime JoinedAtUtc);
