using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.PharmacyOrders.Notifications;
using HealthPlatform.Application.Search;
using HealthPlatform.Domain.Pharmacy;
using MediatR;

namespace HealthPlatform.Application.PharmacyOrders.EventHandlers;

public sealed class OrderStatusChangedNotificationHandler(
    IPatientRepository patientRepository,
    ISearchService searchService,
    IMedicationOrderPatientNotifier notifier)
    : INotificationHandler<OrderStatusChangedNotification>
{
    private const int AlternativePharmacyPageSize = 5;

    public async Task Handle(OrderStatusChangedNotification notification, CancellationToken ct)
    {
        var patient = await patientRepository.GetByIdAsync(notification.PatientId, ct)
            ?? throw new NotFoundException(
                PharmacyErrorCodes.PatientNotFound,
                "Patient profile was not found.");

        IReadOnlyList<PharmacyOrderAlternativeDto>? alternatives = null;
        if (notification.NewStatus == MedicationOrderStatus.Rejected)
        {
            alternatives = await FindAlternativePharmaciesAsync(notification, ct);
        }

        await notifier.NotifyOrderStatusChangedAsync(
            patient.UserId,
            notification.OrderId,
            notification.PreviousStatus.ToString().ToLowerInvariant(),
            notification.NewStatus.ToString().ToLowerInvariant(),
            notification.TrackingUrl,
            alternatives,
            ct);
    }

    private async Task<IReadOnlyList<PharmacyOrderAlternativeDto>> FindAlternativePharmaciesAsync(
        OrderStatusChangedNotification notification,
        CancellationToken ct)
    {
        var page = await searchService.SearchPharmaciesAsync(
            new PharmacySearchCriteria(
                notification.MedicationSku,
                HasStock: true,
                null,
                null,
                Page: 1,
                PageSize: AlternativePharmacyPageSize + 1),
            ct);

        return page.Results
            .Where(match => match.PharmacyId != notification.PharmacyId)
            .Take(AlternativePharmacyPageSize)
            .Select(match => new PharmacyOrderAlternativeDto(
                match.PharmacyId,
                match.Name,
                match.Address))
            .ToList();
    }
}
