using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.UpdateMoodLog;

public sealed record UpdateMoodLogCommand(
    string MoodLogId,
    int Rating,
    string? Notes,
    DateTime? LoggedAtUtc) : ICommand<MoodLogMutationResultDto>;
