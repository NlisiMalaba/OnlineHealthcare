using HealthPlatform.Application.Queue.Realtime;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingQueueRealtimeNotifier : IQueueRealtimeNotifier
{
    public List<QueuePositionUpdatedRealtimeDto> Updates { get; } = [];

    public Task PublishPositionUpdatedAsync(QueuePositionUpdatedRealtimeDto update, CancellationToken ct)
    {
        Updates.Add(update);
        return Task.CompletedTask;
    }
}
