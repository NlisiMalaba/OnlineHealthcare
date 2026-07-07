using Hangfire;
using HealthPlatform.Application.Notifications;
using HealthPlatform.Infrastructure.Jobs;

namespace HealthPlatform.Infrastructure.Notifications;

public sealed class HangfireCriticalNotificationSmsFallbackScheduler : ICriticalNotificationSmsFallbackScheduler
{
    public void EnqueueImmediateProcessing(Guid fallbackId) =>
        BackgroundJob.Enqueue<CriticalNotificationSmsFallbackJob>(
            job => job.ProcessFallback(fallbackId));
}
