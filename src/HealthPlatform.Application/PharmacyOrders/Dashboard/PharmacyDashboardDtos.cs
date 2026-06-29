using HealthPlatform.Application.PharmacyOrders.Inventory;

namespace HealthPlatform.Application.PharmacyOrders.Dashboard;

public sealed record PharmacyIncomingOrderDto(
    Guid OrderId,
    Guid PrescriptionId,
    Guid PatientId,
    string MedicationSku,
    string MedicationName,
    string Dosage,
    string Frequency,
    int DurationDays,
    string? SpecialInstructions,
    string DeliveryType,
    string? DeliveryAddress,
    string Status,
    string PaymentStatus,
    DateTime PlacedAtUtc);

public sealed record PharmacyOrderStatusCountDto(
    string Status,
    int Count);

public sealed record PharmacyDailySummaryDto(
    DateOnly SummaryDate,
    int FulfilledOrderCount,
    decimal RevenueAmount,
    string Currency,
    int PendingDeliveryCount,
    int PendingPickupCount);

public sealed record PharmacyDashboardDto(
    IReadOnlyList<PharmacyIncomingOrderDto> IncomingOrders,
    IReadOnlyList<PharmacyOrderStatusCountDto> OrderStatusCounts,
    IReadOnlyList<InventoryItemDto> InventoryLevels,
    PharmacyDailySummaryDto TodaySummary);
