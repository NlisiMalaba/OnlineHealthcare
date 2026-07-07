using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.Wellness;
using HealthPlatform.Domain.NextOfKin;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingConsecutiveMissedDosesNextOfKinNotifier(INotificationDispatcher dispatcher)
    : IConsecutiveMissedDosesNextOfKinNotifier
{
    public async Task NotifyConsecutiveMissedDosesAsync(
        Guid patientId,
        Guid triggeringAdherenceEventId,
        IReadOnlyList<NextOfKinContactDto> contacts,
        CancellationToken ct)
    {
        var data = new Dictionary<string, string>
        {
            ["patient_id"] = patientId.ToString(),
            ["triggering_adherence_event_id"] = triggeringAdherenceEventId.ToString()
        };

        foreach (var contact in contacts)
        {
            await NotificationRoutingSupport.DispatchToContactAsync(
                dispatcher,
                NotificationRecipientType.NextOfKin,
                NotificationEventTypes.ConsecutiveMissedDoses,
                NotificationCriticality.Critical,
                "Medication adherence alert",
                "A patient has missed multiple consecutive medication doses.",
                new NotificationContactOverride(contact.Email, contact.PhoneNumber),
                data,
                ct);
        }
    }
}
