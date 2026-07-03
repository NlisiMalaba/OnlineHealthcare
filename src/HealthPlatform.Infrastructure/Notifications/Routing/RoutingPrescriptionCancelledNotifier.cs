using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.Prescriptions;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingPrescriptionCancelledNotifier(INotificationDispatcher dispatcher)
    : IPrescriptionCancelledNotifier
{
    public Task NotifyPrescriptionCancelledAsync(
        Guid patientUserId,
        Guid prescriptionId,
        CancellationToken ct) =>
        NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            patientUserId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.PrescriptionCancelled,
            NotificationCriticality.Standard,
            "Prescription cancelled",
            "A prescription has been cancelled.",
            new Dictionary<string, string>
            {
                ["prescription_id"] = prescriptionId.ToString()
            },
            ct);
}
