using HealthPlatform.Application.Identity;
using MediatR;

namespace HealthPlatform.Application.PharmacyOrders.Dashboard.GetPharmacyDailySummary;

public sealed class GetPharmacyDailySummaryQueryHandler(
    ICurrentUserAccessor currentUser,
    IPharmacyRepository pharmacyRepository,
    IPharmacyDashboardRepository dashboardRepository,
    TimeProvider timeProvider)
    : IRequestHandler<GetPharmacyDailySummaryQuery, PharmacyDailySummaryDto>
{
    public async Task<PharmacyDailySummaryDto> Handle(GetPharmacyDailySummaryQuery request, CancellationToken ct)
    {
        var pharmacy = await MedicationOrderWorkflowSupport.ResolveCurrentPharmacyAsync(
            currentUser,
            pharmacyRepository,
            ct);

        var summaryDate = request.SummaryDate
            ?? DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        var summary = await dashboardRepository.GetDailySummaryAsync(pharmacy.Id, summaryDate, ct);
        return summary.ToDailySummaryDto(summaryDate);
    }
}
