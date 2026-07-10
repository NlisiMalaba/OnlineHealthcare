using HealthPlatform.Application.MentalHealth.MoodLogs;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingConsecutiveLowMoodPromptNotifier : IConsecutiveLowMoodPromptNotifier
{
    public List<ConsecutiveLowMoodPromptCall> Calls { get; } = [];

    public Task NotifyPatientAsync(
        Guid patientUserId,
        Guid patientId,
        string triggeringMoodLogId,
        CancellationToken ct)
    {
        Calls.Add(new ConsecutiveLowMoodPromptCall(patientUserId, patientId, triggeringMoodLogId));
        return Task.CompletedTask;
    }

    public sealed record ConsecutiveLowMoodPromptCall(
        Guid PatientUserId,
        Guid PatientId,
        string TriggeringMoodLogId);
}
