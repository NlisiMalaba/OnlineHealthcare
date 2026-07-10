using HealthPlatform.Application.Maternal.AntenatalRecords;
using HealthPlatform.Application.Notifications;
using HealthPlatform.Domain.Maternal;
using HealthPlatform.Infrastructure.Notifications.Routing;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingAntenatalCheckupReminderNotifier(INotificationDispatcher dispatcher)
    : IAntenatalCheckupReminderNotifier
{
    public async Task NotifyAntenatalCheckupReminderAsync(
        Guid patientUserId,
        Guid obstetricDoctorUserId,
        Guid antenatalRecordId,
        DateOnly estimatedDueDate,
        bool highFrequency,
        CancellationToken ct)
    {
        var data = new Dictionary<string, string>
        {
            ["antenatal_record_id"] = antenatalRecordId.ToString(),
            ["estimated_due_date"] = estimatedDueDate.ToString("O"),
            ["high_frequency"] = highFrequency.ToString()
        };

        var patientMessage = highFrequency
            ? "Your estimated due date is approaching. Please stay in close contact with your obstetric care team."
            : "This is a reminder to keep up with your recommended antenatal checkups.";

        var doctorMessage = highFrequency
            ? "A patient's estimated due date is within four weeks. Increased antenatal monitoring reminders are active."
            : "A reminder for your patient's ongoing antenatal care schedule.";

        await NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            patientUserId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.AntenatalCheckupReminder,
            NotificationCriticality.Standard,
            "Antenatal care reminder",
            patientMessage,
            data,
            ct);

        await NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            obstetricDoctorUserId,
            NotificationRecipientType.Doctor,
            NotificationEventTypes.AntenatalCheckupReminder,
            NotificationCriticality.Standard,
            "Antenatal care reminder",
            doctorMessage,
            data,
            ct);
    }
}
