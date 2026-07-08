namespace HealthPlatform.Application.Notifications;

public static class NotificationPolicies
{
    public const int MaxSmsFallbackRetries = 3;

    public const int SmsFallbackBatchSize = 100;

    public static readonly TimeSpan SmsFallbackRetryInterval = TimeSpan.FromMinutes(5);

    public static bool RequiresSmsFallbackOnPushFailure(string eventType) =>
        eventType == NotificationEventTypes.MedicationDoseReminder
        || eventType == NotificationEventTypes.EmergencyAlert;
}
