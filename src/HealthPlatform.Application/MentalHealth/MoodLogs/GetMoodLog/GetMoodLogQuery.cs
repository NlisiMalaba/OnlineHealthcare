using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.GetMoodLog;

public sealed record GetMoodLogQuery(string MoodLogId) : IQuery<MoodLogDto>;
