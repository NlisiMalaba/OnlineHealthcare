namespace HealthPlatform.Domain.Notifications;

public enum CriticalNotificationSmsFallbackStatus
{
    AwaitingProcessing = 0,
    AwaitingRetry = 1,
    Sent = 2,
    FailedFinal = 3
}
