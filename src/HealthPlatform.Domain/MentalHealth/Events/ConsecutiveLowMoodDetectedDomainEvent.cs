using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.MentalHealth.Events;

public sealed record ConsecutiveLowMoodDetectedDomainEvent(
    Guid PatientId,
    string TriggeringMoodLogId,
    DateTime StreakEndLoggedAtUtc) : DomainEvent;
