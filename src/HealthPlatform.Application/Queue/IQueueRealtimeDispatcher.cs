namespace HealthPlatform.Application.Queue;

public interface IQueueRealtimeDispatcher
{
    Task<int> DispatchAsync(CancellationToken ct);
}
