using HealthPlatform.Application.Telemedicine;
using HealthPlatform.Application.Telemedicine.Realtime;
using HealthPlatform.Domain.Telemedicine;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Telemedicine;

public sealed class TelemedicineSessionDurationTickerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Active_session_duration_tick_increases_over_time()
    {
        var timeProvider = new FakeTimeProvider(DateTime.UtcNow);
        await using var host = new PatientRegistrationTestHost(timeProvider: timeProvider);
        var context = await TelemedicineSessionTestContextFactory.CreateActiveAsync(host);

        var session = await host.DbContext.TelemedicineSessions.SingleAsync(s => s.AppointmentId == context.AppointmentId);
        var startedAt = session.StartedAtUtc!.Value;

        timeProvider.SetUtcNow(startedAt.AddSeconds(5));

        var notifier = host.TelemedicineRealtimeNotifier;
        var durationSeconds = Math.Max(0, (int)(timeProvider.GetUtcNow().UtcDateTime - startedAt).TotalSeconds);
        await notifier.PublishDurationTickAsync(
            new TelemedicineDurationTickDto(context.AppointmentId, durationSeconds, timeProvider.GetUtcNow().UtcDateTime),
            CancellationToken.None);

        Assert.Single(notifier.DurationTicks);
        Assert.True(notifier.DurationTicks[0].DurationSeconds >= 5);
    }
}
