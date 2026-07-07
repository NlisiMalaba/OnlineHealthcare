using HealthPlatform.Application.Notifications;
using HealthPlatform.Domain.Notifications;
using HealthPlatform.Infrastructure.Persistence;
using HealthPlatform.Infrastructure.Persistence.Repositories;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HealthPlatform.Tests.Unit.Notifications;

public sealed class CriticalNotificationSmsFallbackServiceTests
{
    private static readonly DateTime ReferenceNowUtc = new(2026, 7, 3, 14, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task ProcessAsync_marks_sent_and_writes_sms_log_when_gateway_succeeds()
    {
        await using var host = CreateHost(smsSucceeds: true);
        var service = host.GetRequiredService<ICriticalNotificationSmsFallbackService>();
        var fallbackId = await SeedFallbackAsync(host.DbContext);

        var succeeded = await service.ProcessAsync(fallbackId, CancellationToken.None);

        Assert.True(succeeded);
        var fallback = await host.DbContext.CriticalNotificationSmsFallbacks.SingleAsync();
        Assert.Equal(CriticalNotificationSmsFallbackStatus.Sent, fallback.Status);
        Assert.Single(await host.DbContext.NotificationLogs.Where(entry => entry.Channel == "sms").ToListAsync());
    }

    [Fact]
    public async Task ProcessAsync_retries_until_max_attempts_then_marks_failed_final()
    {
        await using var host = CreateHost(smsSucceeds: false);
        var service = host.GetRequiredService<ICriticalNotificationSmsFallbackService>();
        var clock = host.Clock;
        var fallbackId = await SeedFallbackAsync(host.DbContext);

        for (var attempt = 0; attempt < NotificationPolicies.MaxSmsFallbackRetries; attempt++)
        {
            await service.ProcessAsync(fallbackId, CancellationToken.None);
            if (attempt < NotificationPolicies.MaxSmsFallbackRetries - 1)
            {
                clock.SetUtcNow(clock.UtcNow.Add(NotificationPolicies.SmsFallbackRetryInterval));
            }
        }

        var fallback = await host.DbContext.CriticalNotificationSmsFallbacks.SingleAsync();
        Assert.Equal(CriticalNotificationSmsFallbackStatus.FailedFinal, fallback.Status);
        Assert.Equal(NotificationPolicies.MaxSmsFallbackRetries, fallback.RetryCount);
    }

    [Fact]
    public async Task ScheduleAsync_persists_fallback_and_enqueues_processing()
    {
        var capturingScheduler = new CapturingCriticalNotificationSmsFallbackScheduler();
        await using var host = CreateHost(smsSucceeds: true, capturingScheduler);
        var service = host.GetRequiredService<ICriticalNotificationSmsFallbackService>();
        var userId = Guid.CreateVersion7();

        await service.ScheduleAsync(
            new NotificationDispatchRequest(
                userId,
                NotificationRecipientType.Patient,
                NotificationEventTypes.EmergencyAlert,
                NotificationCriticality.Critical,
                new NotificationContent("Emergency alert", "Alert body"),
                Metadata: new Dictionary<string, string>
                {
                    ["contact_id"] = userId.ToString()
                }),
            new ResolvedNotificationRecipient(
                userId,
                NotificationRecipientType.Patient,
                "patient@example.com",
                "+263771111111",
                []),
            CancellationToken.None);

        Assert.Single(capturingScheduler.EnqueuedIds);
        await service.ProcessAsync(capturingScheduler.EnqueuedIds[0], CancellationToken.None);

        var fallback = await host.DbContext.CriticalNotificationSmsFallbacks.SingleAsync();
        Assert.Equal(CriticalNotificationSmsFallbackStatus.Sent, fallback.Status);
    }

    [Fact]
    public async Task ProcessDueAsync_processes_due_fallback_from_retry_queue()
    {
        await using var host = CreateHost(smsSucceeds: false);
        var service = host.GetRequiredService<ICriticalNotificationSmsFallbackService>();
        var clock = host.Clock;
        var fallbackId = await SeedFallbackAsync(host.DbContext);

        await service.ProcessAsync(fallbackId, CancellationToken.None);
        clock.SetUtcNow(clock.UtcNow.Add(NotificationPolicies.SmsFallbackRetryInterval));

        var processed = await service.ProcessDueAsync(CancellationToken.None);

        Assert.Equal(1, processed);
        var fallback = await host.DbContext.CriticalNotificationSmsFallbacks.SingleAsync();
        Assert.Equal(2, fallback.RetryCount);
        Assert.Equal(CriticalNotificationSmsFallbackStatus.AwaitingRetry, fallback.Status);
    }

    [Fact]
    public async Task ProcessDueAsync_skips_fallback_not_yet_due()
    {
        await using var host = CreateHost(smsSucceeds: false);
        var service = host.GetRequiredService<ICriticalNotificationSmsFallbackService>();
        var fallbackId = await SeedFallbackAsync(host.DbContext);

        await service.ProcessAsync(fallbackId, CancellationToken.None);

        var processed = await service.ProcessDueAsync(CancellationToken.None);

        Assert.Equal(0, processed);
        var fallback = await host.DbContext.CriticalNotificationSmsFallbacks.SingleAsync();
        Assert.Equal(1, fallback.RetryCount);
        Assert.Equal(CriticalNotificationSmsFallbackStatus.AwaitingRetry, fallback.Status);
    }

    [Fact]
    public async Task ProcessAsync_on_dead_lettered_fallback_does_not_attempt_delivery()
    {
        await using var host = CreateHost(smsSucceeds: false);
        var service = host.GetRequiredService<ICriticalNotificationSmsFallbackService>();
        var clock = host.Clock;
        var fallbackId = await SeedFallbackAsync(host.DbContext);

        for (var attempt = 0; attempt < NotificationPolicies.MaxSmsFallbackRetries; attempt++)
        {
            await service.ProcessAsync(fallbackId, CancellationToken.None);
            if (attempt < NotificationPolicies.MaxSmsFallbackRetries - 1)
            {
                clock.SetUtcNow(clock.UtcNow.Add(NotificationPolicies.SmsFallbackRetryInterval));
            }
        }

        var smsLogsBefore = await host.DbContext.NotificationLogs
            .Where(entry => entry.Channel == "sms")
            .CountAsync();

        var succeeded = await service.ProcessAsync(fallbackId, CancellationToken.None);

        Assert.False(succeeded);
        var fallback = await host.DbContext.CriticalNotificationSmsFallbacks.SingleAsync();
        Assert.Equal(CriticalNotificationSmsFallbackStatus.FailedFinal, fallback.Status);
        Assert.Equal(NotificationPolicies.MaxSmsFallbackRetries, fallback.RetryCount);
        Assert.NotNull(fallback.FinalizedAtUtc);
        Assert.Equal(smsLogsBefore, await host.DbContext.NotificationLogs.CountAsync(entry => entry.Channel == "sms"));
    }

    [Fact]
    public async Task ProcessDueAsync_ignores_dead_lettered_fallbacks()
    {
        await using var host = CreateHost(smsSucceeds: false);
        var service = host.GetRequiredService<ICriticalNotificationSmsFallbackService>();
        var clock = host.Clock;
        var fallbackId = await SeedFallbackAsync(host.DbContext);

        for (var attempt = 0; attempt < NotificationPolicies.MaxSmsFallbackRetries; attempt++)
        {
            await service.ProcessAsync(fallbackId, CancellationToken.None);
            if (attempt < NotificationPolicies.MaxSmsFallbackRetries - 1)
            {
                clock.SetUtcNow(clock.UtcNow.Add(NotificationPolicies.SmsFallbackRetryInterval));
            }
        }

        clock.SetUtcNow(clock.UtcNow.Add(NotificationPolicies.SmsFallbackRetryInterval));
        var processed = await service.ProcessDueAsync(CancellationToken.None);

        Assert.Equal(0, processed);
    }

    private static async Task<Guid> SeedFallbackAsync(ApplicationDbContext dbContext)
    {
        var fallback = CriticalNotificationSmsFallback.CreatePending(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "patient",
            NotificationEventTypes.MedicationDoseReminder,
            "Medication reminder",
            "Take your dose.",
            "{}",
            "patient@example.com",
            "+263771111111",
            ReferenceNowUtc);

        await dbContext.CriticalNotificationSmsFallbacks.AddAsync(fallback);
        await dbContext.SaveChangesAsync();
        return fallback.Id;
    }

    private static NotificationFallbackTestHost CreateHost(
        bool smsSucceeds,
        CapturingCriticalNotificationSmsFallbackScheduler? scheduler = null)
    {
        scheduler ??= new CapturingCriticalNotificationSmsFallbackScheduler();
        var clock = new FakeTimeProvider(ReferenceNowUtc);
        var gatewayConfig = new ControllableNotificationGatewayConfig
        {
            PushSucceeds = false,
            SmsSucceeds = smsSucceeds
        };

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<TimeProvider>(clock);
        services.AddSingleton(clock);
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString("N")));
        services.AddSingleton<INotificationChannelGatewayResolver>(
            new ControllableNotificationChannelGatewayResolver(gatewayConfig));
        services.AddScoped<INotificationLogRepository, NotificationLogRepository>();
        services.AddScoped<INotificationLogWriter, NotificationLogWriter>();
        services.AddScoped<ICriticalNotificationSmsFallbackRepository, CriticalNotificationSmsFallbackRepository>();
        services.AddScoped<ICriticalNotificationSmsFallbackService, CriticalNotificationSmsFallbackService>();
        services.AddSingleton<ICriticalNotificationSmsFallbackScheduler>(scheduler);

        var provider = services.BuildServiceProvider();
        provider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
        return new NotificationFallbackTestHost(provider, clock);
    }

    private sealed class NotificationFallbackTestHost(
        ServiceProvider provider,
        FakeTimeProvider clock) : IAsyncDisposable
    {
        public FakeTimeProvider Clock => clock;

        public ApplicationDbContext DbContext => provider.GetRequiredService<ApplicationDbContext>();

        public T GetRequiredService<T>() where T : notnull => provider.GetRequiredService<T>();

        public ValueTask DisposeAsync() => provider.DisposeAsync();
    }
}
