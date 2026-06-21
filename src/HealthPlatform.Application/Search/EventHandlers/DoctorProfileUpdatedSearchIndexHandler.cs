using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Search;
using HealthPlatform.Application.Search.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Search.EventHandlers;

public sealed class DoctorProfileUpdatedSearchIndexHandler(
    IDoctorRepository doctorRepository,
    ISearchService searchService,
    ILogger<DoctorProfileUpdatedSearchIndexHandler> logger) : INotificationHandler<DoctorProfileUpdatedNotification>
{
    public async Task Handle(DoctorProfileUpdatedNotification notification, CancellationToken ct)
    {
        await DoctorSearchIndexSync.UpsertVerifiedDoctorAsync(
            doctorRepository,
            searchService,
            notification.DoctorId,
            ct);

        logger.LogInformation(
            "Synchronized doctor {DoctorId} profile to search index.",
            notification.DoctorId);
    }
}
