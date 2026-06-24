namespace HealthPlatform.Domain.Identity.Events;

public sealed record PharmacyStockSummaryItem(
    string MedicationName,
    string MedicationSku,
    int QuantityOnHand);
