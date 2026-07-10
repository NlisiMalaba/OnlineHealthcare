using HealthPlatform.API.Hubs;
using HealthPlatform.Application.Queue.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace HealthPlatform.API.Realtime;

public sealed class SignalRQueueRealtimeNotifier(IHubContext<QueueHub> hubContext) : IQueueRealtimeNotifier
{
    public Task PublishPositionUpdatedAsync(QueuePositionUpdatedRealtimeDto update, CancellationToken ct) =>
        hubContext.Clients
            .Group(QueueGroupNames.ForQueueEntry(update.QueueEntryId))
            .SendAsync(QueueHubEvents.PositionUpdated, update, ct);
}
