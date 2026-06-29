using HealthPlatform.Application.PharmacyOrders.Dashboard;
using HealthPlatform.Domain.Pharmacy;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Infrastructure.Persistence.Repositories;

public sealed class PharmacyDashboardRepository(ApplicationDbContext db) : IPharmacyDashboardRepository
{
    public async Task<PharmacyDashboardReadModel> GetDashboardAsync(
        Guid pharmacyId,
        DateOnly todayUtc,
        CancellationToken ct)
    {
        var incomingOrders = await db.MedicationOrders
            .AsNoTracking()
            .Where(order => order.PharmacyId == pharmacyId
                && (order.Status == MedicationOrderStatus.Pending
                    || order.Status == MedicationOrderStatus.ClarificationRequested))
            .OrderByDescending(order => order.CreatedAtUtc)
            .Select(order => new PharmacyDashboardOrderReadModel(
                order.Id,
                order.PrescriptionId,
                order.PatientId,
                order.MedicationSku,
                order.MedicationName,
                order.Dosage,
                order.Frequency,
                order.DurationDays,
                order.SpecialInstructions,
                order.DeliveryType,
                order.DeliveryAddress,
                order.Status,
                order.CreatedAtUtc))
            .ToListAsync(ct);

        var orderStatusCounts = await db.MedicationOrders
            .AsNoTracking()
            .Where(order => order.PharmacyId == pharmacyId)
            .GroupBy(order => order.Status)
            .Select(group => new PharmacyOrderStatusCountReadModel(group.Key, group.Count()))
            .ToListAsync(ct);

        var inventoryLevels = await db.InventoryItems
            .AsNoTracking()
            .Where(item => item.PharmacyId == pharmacyId)
            .OrderBy(item => item.MedicationName)
            .Select(item => new PharmacyDashboardInventoryReadModel(
                item.Id,
                item.PharmacyId,
                item.MedicationName,
                item.MedicationSku,
                item.Quantity,
                item.LowStockThreshold,
                item.UpdatedAtUtc))
            .ToListAsync(ct);

        var todaySummary = await BuildDailySummaryAsync(pharmacyId, todayUtc, ct);

        return new PharmacyDashboardReadModel(
            incomingOrders,
            orderStatusCounts,
            inventoryLevels,
            todaySummary);
    }

    public Task<PharmacyDailySummaryReadModel> GetDailySummaryAsync(
        Guid pharmacyId,
        DateOnly summaryDate,
        CancellationToken ct) =>
        BuildDailySummaryAsync(pharmacyId, summaryDate, ct);

    private async Task<PharmacyDailySummaryReadModel> BuildDailySummaryAsync(
        Guid pharmacyId,
        DateOnly summaryDate,
        CancellationToken ct)
    {
        var dayStart = summaryDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEnd = dayStart.AddDays(1);

        var fulfilledOrderCount = await db.MedicationOrders
            .AsNoTracking()
            .CountAsync(
                order => order.PharmacyId == pharmacyId
                    && order.Status == MedicationOrderStatus.Delivered
                    && order.UpdatedAtUtc >= dayStart
                    && order.UpdatedAtUtc < dayEnd,
                ct);

        var pendingDeliveryCount = await db.MedicationOrders
            .AsNoTracking()
            .CountAsync(
                order => order.PharmacyId == pharmacyId
                    && order.DeliveryType == MedicationDeliveryType.Delivery
                    && (order.Status == MedicationOrderStatus.Confirmed
                        || order.Status == MedicationOrderStatus.Dispatched),
                ct);

        var pendingPickupCount = await db.MedicationOrders
            .AsNoTracking()
            .CountAsync(
                order => order.PharmacyId == pharmacyId
                    && order.DeliveryType == MedicationDeliveryType.Pickup
                    && order.Status == MedicationOrderStatus.Confirmed,
                ct);

        return new PharmacyDailySummaryReadModel(
            fulfilledOrderCount,
            RevenueAmount: 0m,
            pendingDeliveryCount,
            pendingPickupCount);
    }
}
