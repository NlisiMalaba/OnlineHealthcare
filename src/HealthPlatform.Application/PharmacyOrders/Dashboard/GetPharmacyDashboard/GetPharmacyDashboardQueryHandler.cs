using HealthPlatform.Application.Identity;
using MediatR;

namespace HealthPlatform.Application.PharmacyOrders.Dashboard.GetPharmacyDashboard;

public sealed class GetPharmacyDashboardQueryHandler(
    ICurrentUserAccessor currentUser,
    IPharmacyRepository pharmacyRepository,
    IPharmacyDashboardRepository dashboardRepository,
    TimeProvider timeProvider)
    : IRequestHandler<GetPharmacyDashboardQuery, PharmacyDashboardDto>
{
    public async Task<PharmacyDashboardDto> Handle(GetPharmacyDashboardQuery request, CancellationToken ct)
    {
        var pharmacy = await MedicationOrderWorkflowSupport.ResolveCurrentPharmacyAsync(
            currentUser,
            pharmacyRepository,
            ct);

        var todayUtc = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var dashboard = await dashboardRepository.GetDashboardAsync(pharmacy.Id, todayUtc, ct);
        return dashboard.ToDashboardDto(todayUtc);
    }
}
