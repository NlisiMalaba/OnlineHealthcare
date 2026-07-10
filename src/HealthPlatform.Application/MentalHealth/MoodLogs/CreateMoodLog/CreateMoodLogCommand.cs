using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.CreateMoodLog;

public sealed record CreateMoodLogCommand(
    int Rating,
    string? Notes,
    DateTime? LoggedAtUtc) : ICommand<MoodLogMutationResultDto>;
