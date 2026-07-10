using HealthPlatform.Application.Maternal.AntenatalRecords;
using HealthPlatform.Application.Notifications;
using HealthPlatform.Infrastructure.Notifications.Routing;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingFetalMonitoringReminderNotifier(INotificationDispatcher dispatcher)
    : IFetalMonitoringReminderNotifier
{
    public async Task NotifyFetalMonitoringReminderAsync(
        Guid patientUserId,
        Guid antenatalRecordId,
        int intervalDays,
        CancellationToken ct)
    {
        var data = new Dictionary<string, string>
        {
            ["antenatal_record_id"] = antenatalRecordId.ToString(),
            ["interval_days"] = intervalDays.ToString()
        };

        await NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            patientUserId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.FetalMonitoringReminder,
            NotificationCriticality.Standard,
            "Fetal monitoring reminder",
            "Please complete your fetal monitoring check as recommended by your obstetric doctor.",
            data,
            ct);
    }
}
