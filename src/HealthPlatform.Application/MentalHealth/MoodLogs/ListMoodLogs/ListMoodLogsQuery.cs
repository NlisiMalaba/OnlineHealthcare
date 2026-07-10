using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.ListMoodLogs;

public sealed record ListMoodLogsQuery(
    DateTime? FromUtc,
    DateTime? ToUtc) : IQuery<IReadOnlyList<MoodLogDto>>;
