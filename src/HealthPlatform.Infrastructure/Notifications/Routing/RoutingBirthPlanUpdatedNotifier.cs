using HealthPlatform.Application.Maternal.BirthPlans;
using HealthPlatform.Application.Notifications;
using HealthPlatform.Infrastructure.Notifications.Routing;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingBirthPlanUpdatedNotifier(INotificationDispatcher dispatcher)
    : IBirthPlanUpdatedNotifier
{
    public async Task NotifyBirthPlanUpdatedAsync(
        Guid obstetricDoctorUserId,
        Guid birthPlanId,
        Guid antenatalRecordId,
        Guid patientId,
        CancellationToken ct)
    {
        var data = new Dictionary<string, string>
        {
            ["birth_plan_id"] = birthPlanId.ToString(),
            ["antenatal_record_id"] = antenatalRecordId.ToString(),
            ["patient_id"] = patientId.ToString()
        };

        await NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            obstetricDoctorUserId,
            NotificationRecipientType.Doctor,
            NotificationEventTypes.BirthPlanUpdated,
            NotificationCriticality.Standard,
            "Birth plan updated",
            "A patient has updated their birth plan.",
            data,
            ct);
    }
}
