using HealthPlatform.Application.Maternal.GrowthEntries;
using HealthPlatform.Application.Notifications;
using HealthPlatform.Domain.Maternal;
using HealthPlatform.Infrastructure.Notifications.Routing;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingChildGrowthOutOfRangeNotifier(INotificationDispatcher dispatcher)
    : IChildGrowthOutOfRangeNotifier
{
    public Task NotifyGuardianAsync(
        Guid guardianUserId,
        Guid childProfileId,
        Guid growthEntryId,
        string childFullName,
        ChildGrowthMeasurementStatus heightStatus,
        ChildGrowthMeasurementStatus weightStatus,
        CancellationToken ct)
    {
        var data = new Dictionary<string, string>
        {
            ["child_profile_id"] = childProfileId.ToString(),
            ["growth_entry_id"] = growthEntryId.ToString(),
            ["child_full_name"] = childFullName,
            ["height_status"] = heightStatus.ToString(),
            ["weight_status"] = weightStatus.ToString(),
            ["pediatric_consultation_path"] = "/appointments/bookings"
        };

        return NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            guardianUserId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.ChildGrowthOutOfRangeAlert,
            NotificationCriticality.Standard,
            "Child growth measurement alert",
            $"A recent growth measurement for {childFullName} is outside the expected range for their age. Consider booking a consultation with a pediatric doctor.",
            data,
            ct);
    }
}
