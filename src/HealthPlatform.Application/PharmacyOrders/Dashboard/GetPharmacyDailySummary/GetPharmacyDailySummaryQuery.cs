using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.PharmacyOrders.Dashboard.GetPharmacyDailySummary;

public sealed record GetPharmacyDailySummaryQuery(DateOnly? SummaryDate) : IQuery<PharmacyDailySummaryDto>;
