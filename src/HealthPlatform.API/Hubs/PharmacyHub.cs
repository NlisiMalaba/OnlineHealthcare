using HealthPlatform.Application.PharmacyOrders.Realtime;
using HealthPlatform.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HealthPlatform.API.Hubs;

[Authorize(Policy = AuthorizationPolicies.Pharmacy)]
public sealed class PharmacyHub : Hub
{
    public async Task JoinPharmacyAsync(Guid pharmacyId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await Groups.AddToGroupAsync(Context.ConnectionId, PharmacyGroupNames.ForPharmacy(pharmacyId), ct);
    }
}
