using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Identity.Events;

public sealed record PharmacyStockChangedDomainEvent(
    Guid PharmacyId,
    IReadOnlyList<PharmacyStockSummaryItem> StockSummary) : DomainEvent;
