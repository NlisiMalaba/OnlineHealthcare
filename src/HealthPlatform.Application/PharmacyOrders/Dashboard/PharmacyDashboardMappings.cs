using HealthPlatform.Application.PharmacyOrders.Inventory;
using HealthPlatform.Domain.Pharmacy;

namespace HealthPlatform.Application.PharmacyOrders.Dashboard;

internal static class PharmacyDashboardMappings
{
    public static PharmacyIncomingOrderDto ToIncomingOrderDto(this PharmacyDashboardOrderReadModel order) =>
        new(
            order.OrderId,
            order.PrescriptionId,
            order.PatientId,
            order.MedicationSku,
            order.MedicationName,
            order.Dosage,
            order.Frequency,
            order.DurationDays,
            order.SpecialInstructions,
            order.DeliveryType.ToString().ToLowerInvariant(),
            order.DeliveryAddress,
            order.Status.ToString().ToLowerInvariant(),
            PharmacyDashboardPolicies.PaymentStatusPending,
            order.CreatedAtUtc);

    public static PharmacyOrderStatusCountDto ToStatusCountDto(this PharmacyOrderStatusCountReadModel count) =>
        new(
            count.Status.ToString().ToLowerInvariant(),
            count.Count);

    public static InventoryItemDto ToInventoryDto(this PharmacyDashboardInventoryReadModel item) =>
        new(
            item.Id,
            item.PharmacyId,
            item.MedicationName,
            item.MedicationSku,
            item.Quantity,
            item.LowStockThreshold,
            item.Quantity == 0,
            item.UpdatedAtUtc);

    public static PharmacyDailySummaryDto ToDailySummaryDto(
        this PharmacyDailySummaryReadModel summary,
        DateOnly summaryDate) =>
        new(
            summaryDate,
            summary.FulfilledOrderCount,
            summary.RevenueAmount,
            PharmacyDashboardPolicies.DefaultReportCurrency,
            summary.PendingDeliveryCount,
            summary.PendingPickupCount);

    public static PharmacyDashboardDto ToDashboardDto(this PharmacyDashboardReadModel dashboard, DateOnly todayUtc) =>
        new(
            dashboard.IncomingOrders.Select(order => order.ToIncomingOrderDto()).ToList(),
            dashboard.OrderStatusCounts.Select(count => count.ToStatusCountDto()).ToList(),
            dashboard.InventoryLevels.Select(item => item.ToInventoryDto()).ToList(),
            dashboard.TodaySummary.ToDailySummaryDto(todayUtc));
}
