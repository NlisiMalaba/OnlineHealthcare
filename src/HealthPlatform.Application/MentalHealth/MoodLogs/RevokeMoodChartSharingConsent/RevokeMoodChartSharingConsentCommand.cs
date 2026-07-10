using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.RevokeMoodChartSharingConsent;

public sealed record RevokeMoodChartSharingConsentCommand(Guid TherapistId) : ICommand<MoodChartSharingConsentDto>;
