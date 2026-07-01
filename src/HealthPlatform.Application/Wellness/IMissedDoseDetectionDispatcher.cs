namespace HealthPlatform.Application.Wellness;

public interface IMissedDoseDetectionDispatcher
{
    Task<int> RecordMissedDosesAsync(CancellationToken ct);
}
