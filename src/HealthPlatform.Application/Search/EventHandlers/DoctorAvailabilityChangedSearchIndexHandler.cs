using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.Notifications;
using HealthPlatform.Application.Search;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Search.EventHandlers;

public sealed class DoctorAvailabilityChangedSearchIndexHandler(
    IDoctorRepository doctorRepository,
    ISearchService searchService,
    ILogger<DoctorAvailabilityChangedSearchIndexHandler> logger) : INotificationHandler<DoctorAvailabilityChangedNotification>
{
    public async Task Handle(DoctorAvailabilityChangedNotification notification, CancellationToken ct)
    {
        await DoctorSearchIndexSync.UpsertVerifiedDoctorAsync(
            doctorRepository,
            searchService,
            notification.DoctorId,
            ct);

        logger.LogInformation(
            "Synchronized doctor {DoctorId} availability to search index.",
            notification.DoctorId);
    }
}
