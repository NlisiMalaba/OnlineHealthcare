using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.DeleteMoodLog;

public sealed record DeleteMoodLogCommand(string MoodLogId) : ICommand;
