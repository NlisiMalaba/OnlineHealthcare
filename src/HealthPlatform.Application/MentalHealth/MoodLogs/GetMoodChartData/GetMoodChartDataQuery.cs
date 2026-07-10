using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.GetMoodChartData;

public sealed record GetMoodChartDataQuery(
    DateTime? FromUtc,
    DateTime? ToUtc) : IQuery<MoodChartDataDto>;
