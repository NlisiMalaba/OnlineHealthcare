using HealthPlatform.API.Hubs;
using HealthPlatform.Application.PharmacyOrders.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace HealthPlatform.API.Realtime;

public sealed class SignalRPharmacyOrderRealtimeNotifier(IHubContext<PharmacyHub> hubContext)
    : IPharmacyOrderRealtimeNotifier
{
    public Task PublishOrderReceivedAsync(
        Guid pharmacyId,
        PharmacyOrderReceivedRealtimeDto order,
        CancellationToken ct) =>
        hubContext.Clients
            .Group(PharmacyGroupNames.ForPharmacy(pharmacyId))
            .SendAsync(PharmacyHubEvents.OrderReceived, order, ct);
}
