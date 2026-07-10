using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.GrantMoodChartSharingConsent;

public sealed record GrantMoodChartSharingConsentCommand(Guid TherapistId) : ICommand<MoodChartSharingConsentDto>;
