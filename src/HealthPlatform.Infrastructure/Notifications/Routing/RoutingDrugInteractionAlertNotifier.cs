using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.Prescriptions;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingDrugInteractionAlertNotifier(INotificationDispatcher dispatcher)
    : IDrugInteractionAlertNotifier
{
    public Task NotifyDrugInteractionAlertAsync(
        Guid doctorUserId,
        Guid patientId,
        string proposedMedicationName,
        string interactingMedicationName,
        string interactionDescription,
        CancellationToken ct) =>
        NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            doctorUserId,
            NotificationRecipientType.Doctor,
            NotificationEventTypes.DrugInteractionAlert,
            NotificationCriticality.Standard,
            "Drug interaction alert",
            "Review the proposed prescription before finalizing.",
            new Dictionary<string, string>
            {
                ["patient_id"] = patientId.ToString()
            },
            ct);
}
