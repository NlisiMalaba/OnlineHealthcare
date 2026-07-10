namespace HealthPlatform.Application.MentalHealth.MoodLogs;

public interface IConsecutiveLowMoodPromptNotifier
{
    Task NotifyPatientAsync(
        Guid patientUserId,
        Guid patientId,
        string triggeringMoodLogId,
        CancellationToken ct);
}
