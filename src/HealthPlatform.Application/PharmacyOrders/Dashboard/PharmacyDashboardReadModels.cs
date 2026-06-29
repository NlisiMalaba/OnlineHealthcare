using HealthPlatform.Domain.Pharmacy;

namespace HealthPlatform.Application.PharmacyOrders.Dashboard;

public sealed record PharmacyDashboardOrderReadModel(
    Guid OrderId,
    Guid PrescriptionId,
    Guid PatientId,
    string MedicationSku,
    string MedicationName,
    string Dosage,
    string Frequency,
    int DurationDays,
    string? SpecialInstructions,
    MedicationDeliveryType DeliveryType,
    string? DeliveryAddress,
    MedicationOrderStatus Status,
    DateTime CreatedAtUtc);

public sealed record PharmacyOrderStatusCountReadModel(
    MedicationOrderStatus Status,
    int Count);

public sealed record PharmacyDailySummaryReadModel(
    int FulfilledOrderCount,
    decimal RevenueAmount,
    int PendingDeliveryCount,
    int PendingPickupCount);

public sealed record PharmacyDashboardReadModel(
    IReadOnlyList<PharmacyDashboardOrderReadModel> IncomingOrders,
    IReadOnlyList<PharmacyOrderStatusCountReadModel> OrderStatusCounts,
    IReadOnlyList<PharmacyDashboardInventoryReadModel> InventoryLevels,
    PharmacyDailySummaryReadModel TodaySummary);

public sealed record PharmacyDashboardInventoryReadModel(
    Guid Id,
    Guid PharmacyId,
    string MedicationName,
    string MedicationSku,
    int Quantity,
    int LowStockThreshold,
    DateTime UpdatedAtUtc);
