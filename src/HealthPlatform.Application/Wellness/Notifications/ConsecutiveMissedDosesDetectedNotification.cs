using HealthPlatform.Application.NextOfKin;
using MediatR;

namespace HealthPlatform.Application.Wellness.Notifications;

public sealed record ConsecutiveMissedDosesDetectedNotification(
    Guid PatientId,
    Guid TriggeringAdherenceEventId,
    DateTime StreakEndScheduledAtUtc,
    DateTime OccurredAtUtc) : INotification;
