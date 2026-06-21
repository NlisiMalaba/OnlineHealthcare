using HealthPlatform.Application.Identity.Notifications;
using HealthPlatform.Application.Search;
using HealthPlatform.Domain.Identity;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Identity.EventHandlers;

public sealed class DoctorAvailabilityChangedNotificationHandler(
    IDoctorRepository doctorRepository,
    ISearchService searchService,
    ILogger<DoctorAvailabilityChangedNotificationHandler> logger) : INotificationHandler<DoctorAvailabilityChangedNotification>
{
    public async Task Handle(DoctorAvailabilityChangedNotification notification, CancellationToken ct)
    {
        var doctor = await doctorRepository.GetByIdWithSlotsAsync(notification.DoctorId, ct);
        if (doctor is null)
        {
            logger.LogWarning(
                "Skipping search index update for missing doctor {DoctorId}.",
                notification.DoctorId);
            return;
        }

        if (doctor.VerificationStatus != DoctorVerificationStatus.Verified)
        {
            logger.LogDebug(
                "Skipping search index update for unverified doctor {DoctorId}.",
                notification.DoctorId);
            return;
        }

        var entries = doctor.AvailabilitySlots
            .Select(slot => new DoctorAvailabilityIndexEntry(
                slot.DayOfWeek,
                slot.StartTime,
                slot.EndTime,
                slot.SlotDurationMinutes,
                slot.AppointmentType,
                slot.IsActive))
            .ToList();

        await searchService.UpdateDoctorAvailabilityIndexAsync(doctor.Id, entries, ct);

        logger.LogInformation(
            "Updated doctor {DoctorId} availability search index with {SlotCount} slots.",
            doctor.Id,
            entries.Count);
    }
}
