using FsCheck.Xunit;
using HealthPlatform.Application.Notifications;
using HealthPlatform.Infrastructure.Persistence;
using HealthPlatform.Infrastructure.Persistence.Repositories;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace HealthPlatform.Tests.Properties;

public sealed class NotificationLogCompletenessPropertyTests
{
    private static readonly DateTime ReferenceNowUtc = new(2026, 7, 3, 12, 0, 0, DateTimeKind.Utc);

    // Feature: online-healthcare-platform, Property 29: Notification Log Completeness
    [Property(Arbitrary = [typeof(NotificationLogArbitraries)], MaxTest = 100)]
    public bool Every_sent_notification_has_log_entry_with_status_timestamp_and_channel(
        NotificationDispatchCase input) =>
        RunLogCompletenessInvariantAsync(input).GetAwaiter().GetResult();

    private static async Task<bool> RunLogCompletenessInvariantAsync(NotificationDispatchCase input)
    {
        var userId = Guid.CreateVersion7();
        var referenceId = Guid.CreateVersion7();
        var gatewayConfig = new ControllableNotificationGatewayConfig
        {
            PushSucceeds = input.PushSucceeds,
            SmsSucceeds = input.SmsSucceeds,
            EmailSucceeds = input.EmailSucceeds
        };

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<TimeProvider>(new FakeTimeProvider(ReferenceNowUtc));
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString("N")));
        services.AddSingleton<INotificationChannelGatewayResolver>(
            new ControllableNotificationChannelGatewayResolver(gatewayConfig));
        services.AddSingleton<INotificationPreferenceResolver, DefaultNotificationPreferenceResolver>();
        services.AddSingleton<INotificationRecipientResolver>(new FixedNotificationRecipientResolver());
        services.AddScoped<INotificationLogRepository, NotificationLogRepository>();
        services.AddScoped<INotificationLogWriter, NotificationLogWriter>();
        services.AddSingleton<ICriticalNotificationSmsFallbackService>(new Mock<ICriticalNotificationSmsFallbackService>().Object);
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var dispatcher = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var dispatchResult = await dispatcher.DispatchAsync(
            new NotificationDispatchRequest(
                userId,
                input.RecipientType,
                input.EventType,
                NotificationCriticality.Standard,
                new NotificationContent("Property test title", "Property test body"),
                Metadata: new Dictionary<string, string>
                {
                    ["reference_id"] = referenceId.ToString()
                },
                Channels: input.Channels),
            CancellationToken.None);

        if (dispatchResult.ChannelResults.Count != input.Channels.Count)
        {
            return false;
        }

        var logs = await dbContext.NotificationLogs
            .AsNoTracking()
            .Where(entry => entry.RecipientId == userId && entry.EventType == input.EventType)
            .ToListAsync();

        if (logs.Count != dispatchResult.ChannelResults.Count)
        {
            return false;
        }

        foreach (var channelResult in dispatchResult.ChannelResults)
        {
            var channelKey = NotificationLogMappings.ToChannelKey(channelResult.Channel);
            var matchingLog = logs.SingleOrDefault(entry => entry.Channel == channelKey);
            if (matchingLog is null)
            {
                return false;
            }

            var expectedStatus = NotificationLogMappings.ToDeliveryStatus(channelResult.Succeeded);
            if (matchingLog.Status != expectedStatus)
            {
                return false;
            }

            if (matchingLog.SentAtUtc != ReferenceNowUtc)
            {
                return false;
            }

            if (matchingLog.Attempts < 1)
            {
                return false;
            }

            if (!matchingLog.PayloadJson.Contains(referenceId.ToString(), StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private sealed class FixedNotificationRecipientResolver : INotificationRecipientResolver
    {
        public Task<ResolvedNotificationRecipient> ResolveAsync(
            Guid userId,
            NotificationRecipientType recipientType,
            CancellationToken ct) =>
            Task.FromResult(new ResolvedNotificationRecipient(
                userId,
                recipientType,
                "property-test@example.com",
                "+263771234567",
                ["property-test-token"]));
    }
}
