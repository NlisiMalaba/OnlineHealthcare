using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Maternal.ChildProfiles;
using HealthPlatform.Application.Maternal.GrowthEntries.Notifications;
using MediatR;

namespace HealthPlatform.Application.Maternal.GrowthEntries.EventHandlers;

public sealed class ChildGrowthOutOfRangeDetectedNotificationHandler(
    IPatientRepository patientRepository,
    IChildProfileRepository childProfileRepository,
    IChildGrowthOutOfRangeNotifier notifier)
    : INotificationHandler<ChildGrowthOutOfRangeDetectedNotification>
{
    public async Task Handle(ChildGrowthOutOfRangeDetectedNotification notification, CancellationToken ct)
    {
        var guardian = await patientRepository.GetByIdAsync(notification.GuardianId, ct);
        if (guardian is null)
        {
            return;
        }

        var childProfile = await childProfileRepository.GetByIdAsync(notification.ChildProfileId, ct);
        if (childProfile is null)
        {
            return;
        }

        await notifier.NotifyGuardianAsync(
            guardian.UserId,
            notification.ChildProfileId,
            notification.GrowthEntryId,
            childProfile.FullName,
            notification.HeightStatus,
            notification.WeightStatus,
            ct);
    }
}
