using HealthPlatform.Application.MentalHealth.MoodLogs;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.MentalHealth;

public sealed class LoggingConsecutiveLowMoodPromptNotifier(
    ILogger<LoggingConsecutiveLowMoodPromptNotifier> logger) : IConsecutiveLowMoodPromptNotifier
{
    public Task NotifyPatientAsync(
        Guid patientUserId,
        Guid patientId,
        string triggeringMoodLogId,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Consecutive low mood prompt requested for patient {PatientId}, mood log {MoodLogId}, user {UserId}.",
            patientId,
            triggeringMoodLogId,
            patientUserId);
        return Task.CompletedTask;
    }
}
