namespace HealthPlatform.Application.Notifications;

public interface ICriticalNotificationSmsFallbackScheduler
{
    void EnqueueImmediateProcessing(Guid fallbackId);
}
