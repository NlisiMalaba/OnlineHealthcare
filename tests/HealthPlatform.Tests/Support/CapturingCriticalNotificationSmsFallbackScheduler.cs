using HealthPlatform.Application.Notifications;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingCriticalNotificationSmsFallbackScheduler : ICriticalNotificationSmsFallbackScheduler
{
    public List<Guid> EnqueuedIds { get; } = [];

    public void EnqueueImmediateProcessing(Guid fallbackId) => EnqueuedIds.Add(fallbackId);
}
