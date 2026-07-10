using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.Vaccinations;
using HealthPlatform.Infrastructure.Notifications.Routing;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingVaccinationReminderNotifier(INotificationDispatcher dispatcher)
    : IVaccinationReminderNotifier
{
    public async Task NotifyVaccinationDueAsync(
        Guid recipientUserId,
        Guid? childProfileId,
        Guid? patientId,
        Guid scheduleEntryId,
        string vaccineName,
        DateOnly recommendedDate,
        bool isChildProfile,
        CancellationToken ct)
    {
        var data = new Dictionary<string, string>
        {
            ["schedule_entry_id"] = scheduleEntryId.ToString(),
            ["vaccine_name"] = vaccineName,
            ["recommended_date"] = recommendedDate.ToString("O"),
            ["is_child_profile"] = isChildProfile.ToString()
        };

        if (childProfileId.HasValue)
        {
            data["child_profile_id"] = childProfileId.Value.ToString();
        }

        if (patientId.HasValue)
        {
            data["patient_id"] = patientId.Value.ToString();
        }

        var message = isChildProfile
            ? $"A vaccination ({vaccineName}) for your child is due on {recommendedDate:yyyy-MM-dd}."
            : $"Your recommended vaccination ({vaccineName}) is due on {recommendedDate:yyyy-MM-dd}.";

        await NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            recipientUserId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.VaccinationDueReminder,
            NotificationCriticality.Standard,
            "Vaccination reminder",
            message,
            data,
            ct);
    }
}
