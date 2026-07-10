using MediatR;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.Notifications;

public sealed record ConsecutiveLowMoodDetectedNotification(
    Guid PatientId,
    string TriggeringMoodLogId,
    DateTime StreakEndLoggedAtUtc,
    DateTime OccurredAtUtc) : INotification;
