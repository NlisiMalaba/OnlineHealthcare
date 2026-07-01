using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Wellness.Events;

public sealed record ConsecutiveMissedDosesDetectedDomainEvent(
    Guid PatientId,
    Guid TriggeringAdherenceEventId,
    DateTime StreakEndScheduledAtUtc) : DomainEvent;
