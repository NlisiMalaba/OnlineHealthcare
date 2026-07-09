using HealthPlatform.Application.Queue.Realtime;
using HealthPlatform.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HealthPlatform.API.Hubs;

[Authorize(Policy = AuthorizationPolicies.Patient)]
public sealed class QueueHub : Hub
{
    public Task JoinQueueEntryAsync(Guid queueEntryId, CancellationToken ct) =>
        Groups.AddToGroupAsync(Context.ConnectionId, QueueGroupNames.ForQueueEntry(queueEntryId), ct);
}
