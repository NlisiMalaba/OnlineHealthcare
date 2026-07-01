namespace HealthPlatform.Application.Wellness;

public interface IMedicationScheduleCompletionService
{
    Task EvaluateCompletionAsync(Guid scheduleId, CancellationToken ct);
}
