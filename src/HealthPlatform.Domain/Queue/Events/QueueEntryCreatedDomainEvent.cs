using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Queue.Events;

public sealed record QueueEntryCreatedDomainEvent(
    Guid QueueEntryId,
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    int QueuePosition,
    int EstimatedWaitMinutes,
    DateTime JoinedAtUtc) : DomainEvent;
