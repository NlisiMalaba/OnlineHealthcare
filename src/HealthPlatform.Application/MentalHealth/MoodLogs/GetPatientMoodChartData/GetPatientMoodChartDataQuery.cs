using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.GetPatientMoodChartData;

public sealed record GetPatientMoodChartDataQuery(
    Guid PatientId,
    DateTime? FromUtc,
    DateTime? ToUtc) : IQuery<MoodChartDataDto>;
