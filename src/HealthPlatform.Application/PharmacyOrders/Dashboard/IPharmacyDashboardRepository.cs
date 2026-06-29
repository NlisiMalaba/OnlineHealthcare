namespace HealthPlatform.Application.PharmacyOrders.Dashboard;

public interface IPharmacyDashboardRepository
{
    Task<PharmacyDashboardReadModel> GetDashboardAsync(Guid pharmacyId, DateOnly todayUtc, CancellationToken ct);

    Task<PharmacyDailySummaryReadModel> GetDailySummaryAsync(
        Guid pharmacyId,
        DateOnly summaryDate,
        CancellationToken ct);
}
