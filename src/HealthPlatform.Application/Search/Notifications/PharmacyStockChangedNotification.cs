using HealthPlatform.Domain.Identity.Events;
using MediatR;

namespace HealthPlatform.Application.Search.Notifications;

public sealed record PharmacyStockChangedNotification(
    Guid PharmacyId,
    IReadOnlyList<PharmacyStockSummaryItem> StockSummary,
    DateTime OccurredAtUtc) : INotification;
