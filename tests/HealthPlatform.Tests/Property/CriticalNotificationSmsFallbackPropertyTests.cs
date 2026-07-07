using FsCheck.Xunit;
using HealthPlatform.Application.Notifications;
using HealthPlatform.Domain.Notifications;
using HealthPlatform.Infrastructure.Persistence;
using HealthPlatform.Infrastructure.Persistence.Repositories;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HealthPlatform.Tests.Properties;

public sealed class CriticalNotificationSmsFallbackPropertyTests
{
    private static readonly DateTime ReferenceNowUtc = new(2026, 7, 3, 13, 0, 0, DateTimeKind.Utc);

    // Feature: online-healthcare-platform, Property 30: Critical Notification SMS Fallback
    [Property(Arbitrary = [typeof(CriticalNotificationSmsFallbackArbitraries)], MaxTest = 100)]
    public bool Push_failure_on_critical_notification_attempts_sms_fallback(
        CriticalNotificationSmsFallbackCase input) =>
        RunSmsFallbackInvariantAsync(input).GetAwaiter().GetResult();

    private static async Task<bool> RunSmsFallbackInvariantAsync(CriticalNotificationSmsFallbackCase input)
    {
        var userId = Guid.CreateVersion7();
        var referenceId = Guid.CreateVersion7();
        var clock = new FakeTimeProvider(ReferenceNowUtc);
        var capturingScheduler = new CapturingCriticalNotificationSmsFallbackScheduler();
        var smsGateway = new TrackingSmsNotificationGateway { Succeeds = input.SmsSucceeds };
        var gatewayConfig = new ControllableNotificationGatewayConfig
        {
            PushSucceeds = input.PushSucceeds,
            EmailSucceeds = true
        };
        var gatewayResolver = new TrackingNotificationChannelGatewayResolver(gatewayConfig, smsGateway);

        var channels = BuildChannels(input);
        var shouldScheduleFallback = ShouldScheduleFallback(input);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<TimeProvider>(clock);
        services.AddSingleton(clock);
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString("N")));
        services.AddSingleton<INotificationChannelGatewayResolver>(gatewayResolver);
        services.AddSingleton<INotificationPreferenceResolver, DefaultNotificationPreferenceResolver>();
        services.AddSingleton<INotificationRecipientResolver>(new FixedNotificationRecipientResolver());
        services.AddScoped<INotificationLogRepository, NotificationLogRepository>();
        services.AddScoped<INotificationLogWriter, NotificationLogWriter>();
        services.AddScoped<ICriticalNotificationSmsFallbackRepository, CriticalNotificationSmsFallbackRepository>();
        services.AddScoped<ICriticalNotificationSmsFallbackService, CriticalNotificationSmsFallbackService>();
        services.AddSingleton<ICriticalNotificationSmsFallbackScheduler>(capturingScheduler);
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var scopedProvider = scope.ServiceProvider;

        var dispatcher = scopedProvider.GetRequiredService<INotificationDispatcher>();
        var fallbackService = scopedProvider.GetRequiredService<ICriticalNotificationSmsFallbackService>();
        var dbContext = scopedProvider.GetRequiredService<ApplicationDbContext>();

        var dispatchResult = await dispatcher.DispatchAsync(
            new NotificationDispatchRequest(
                userId,
                NotificationRecipientType.Patient,
                input.EventType,
                NotificationCriticality.Critical,
                new NotificationContent("Critical property title", "Critical property body"),
                Metadata: new Dictionary<string, string>
                {
                    ["reference_id"] = referenceId.ToString()
                },
                Channels: channels),
            CancellationToken.None);

        var fallbackCount = await dbContext.CriticalNotificationSmsFallbacks.CountAsync();
        if (shouldScheduleFallback)
        {
            if (fallbackCount != 1 || capturingScheduler.EnqueuedIds.Count != 1)
            {
                return false;
            }

            var fallback = await dbContext.CriticalNotificationSmsFallbacks.SingleAsync();
            if (fallback.EventType != input.EventType
                || fallback.UserId != userId
                || fallback.Status != CriticalNotificationSmsFallbackStatus.AwaitingProcessing)
            {
                return false;
            }

            var smsAttemptsBeforeFallback = smsGateway.AttemptCount;
            var fallbackSucceeded = await fallbackService.ProcessAsync(
                capturingScheduler.EnqueuedIds[0],
                CancellationToken.None);

            if (smsGateway.AttemptCount <= smsAttemptsBeforeFallback)
            {
                return false;
            }

            if (fallbackSucceeded != input.SmsSucceeds)
            {
                return false;
            }

            var smsLogs = await dbContext.NotificationLogs
                .AsNoTracking()
                .Where(entry => entry.RecipientId == userId && entry.Channel == "sms")
                .ToListAsync();

            var expectedSmsLogCount = channels.Contains(NotificationChannel.Sms)
                ? smsGateway.AttemptCount
                : 1;

            if (smsLogs.Count < expectedSmsLogCount)
            {
                return false;
            }

            var updatedFallback = await dbContext.CriticalNotificationSmsFallbacks.SingleAsync();
            var expectedStatus = input.SmsSucceeds
                ? CriticalNotificationSmsFallbackStatus.Sent
                : CriticalNotificationSmsFallbackStatus.AwaitingRetry;

            if (updatedFallback.Status != expectedStatus)
            {
                return false;
            }
        }
        else
        {
            if (fallbackCount != 0 || capturingScheduler.EnqueuedIds.Count != 0)
            {
                return false;
            }

            if (input.IncludeSmsChannel && !input.PushSucceeds)
            {
                if (!dispatchResult.ChannelResults.Any(result =>
                        result.Channel == NotificationChannel.Sms))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static IReadOnlyList<NotificationChannel> BuildChannels(CriticalNotificationSmsFallbackCase input)
    {
        var channels = new List<NotificationChannel> { NotificationChannel.Push };
        if (input.IncludeSmsChannel)
        {
            channels.Add(NotificationChannel.Sms);
        }

        return channels;
    }

    private static bool ShouldScheduleFallback(CriticalNotificationSmsFallbackCase input) =>
        NotificationPolicies.RequiresSmsFallbackOnPushFailure(input.EventType)
        && !input.PushSucceeds
        && !(input.IncludeSmsChannel && input.SmsSucceeds);

    private sealed class FixedNotificationRecipientResolver : INotificationRecipientResolver
    {
        public Task<ResolvedNotificationRecipient> ResolveAsync(
            Guid userId,
            NotificationRecipientType recipientType,
            CancellationToken ct) =>
            Task.FromResult(new ResolvedNotificationRecipient(
                userId,
                recipientType,
                "critical-fallback@example.com",
                "+263771234567",
                ["critical-fallback-token"]));
    }
}
