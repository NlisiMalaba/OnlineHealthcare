using HealthPlatform.Application.MentalHealth.MoodLogs;
using HealthPlatform.Application.Notifications;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingConsecutiveLowMoodPromptNotifier(INotificationDispatcher dispatcher)
    : IConsecutiveLowMoodPromptNotifier
{
    public Task NotifyPatientAsync(
        Guid patientUserId,
        Guid patientId,
        string triggeringMoodLogId,
        CancellationToken ct) =>
        NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            patientUserId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.ConsecutiveLowMoodPrompt,
            NotificationCriticality.Standard,
            "Mental health support available",
            "We've noticed you've been having a difficult time. Explore mental health resources or book a therapy session when you're ready.",
            new Dictionary<string, string>
            {
                ["patient_id"] = patientId.ToString(),
                ["triggering_mood_log_id"] = triggeringMoodLogId,
                ["therapy_booking_path"] = "/appointments/bookings"
            },
            ct);
}
