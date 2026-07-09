namespace HealthPlatform.API.Requests.Queue;

public sealed class RecalculateQueueOnDelayRequest
{
    public int DelayMinutes { get; init; }
}
