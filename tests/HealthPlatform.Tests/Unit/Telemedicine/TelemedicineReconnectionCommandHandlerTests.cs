using HealthPlatform.Application.Telemedicine.Realtime.Reconnection;
using HealthPlatform.Domain.Telemedicine;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Telemedicine;

public sealed class TelemedicineReconnectionCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Disconnect_starts_reconnection_grace_and_publishes_attempting_event()
    {
        var context = await TelemedicineSessionTestContextFactory.CreateActiveAsync(_host);
        _host.CurrentUser.UserId = context.PatientUserId;

        await _host.Sender.Send(
            new NotifyTelemedicineParticipantDisconnectedCommand(context.AppointmentId),
            CancellationToken.None);

        var session = await _host.DbContext.TelemedicineSessions.SingleAsync();
        Assert.NotNull(session.InterruptedAtUtc);
        Assert.Single(_host.TelemedicineRealtimeNotifier.ReconnectionAttempts);
    }

    [Fact]
    public async Task Reconnect_within_grace_clears_interruption_and_publishes_success()
    {
        var timeProvider = new FakeTimeProvider(DateTime.UtcNow);
        await using var host = new PatientRegistrationTestHost(timeProvider: timeProvider);
        var context = await TelemedicineSessionTestContextFactory.CreateActiveAsync(host);
        host.CurrentUser.UserId = context.PatientUserId;

        await host.Sender.Send(
            new NotifyTelemedicineParticipantDisconnectedCommand(context.AppointmentId),
            CancellationToken.None);

        timeProvider.SetUtcNow(timeProvider.UtcNow.AddSeconds(20));
        host.CurrentUser.UserId = context.DoctorUserId;

        var reconnected = await host.Sender.Send(
            new CompleteTelemedicineReconnectionCommand(context.AppointmentId),
            CancellationToken.None);

        Assert.True(reconnected);
        var session = await host.DbContext.TelemedicineSessions.SingleAsync();
        Assert.Null(session.InterruptedAtUtc);
        Assert.Single(host.TelemedicineRealtimeNotifier.ReconnectionSuccesses);
    }

    [Fact]
    public async Task Expired_grace_marks_session_interrupted_and_publishes_prompt()
    {
        var timeProvider = new FakeTimeProvider(DateTime.UtcNow);
        await using var host = new PatientRegistrationTestHost(timeProvider: timeProvider);
        var context = await TelemedicineSessionTestContextFactory.CreateActiveAsync(host);
        host.CurrentUser.UserId = context.PatientUserId;

        await host.Sender.Send(
            new NotifyTelemedicineParticipantDisconnectedCommand(context.AppointmentId),
            CancellationToken.None);

        timeProvider.SetUtcNow(timeProvider.UtcNow.AddSeconds(60));

        var expiredCount = await host.Sender.Send(
            new ProcessExpiredTelemedicineReconnectionGracesCommand(),
            CancellationToken.None);

        Assert.Equal(1, expiredCount);
        var session = await host.DbContext.TelemedicineSessions.SingleAsync();
        Assert.Equal(TelemedicineSessionStatus.Interrupted, session.Status);
        Assert.Single(host.TelemedicineRealtimeNotifier.ReconnectionPrompts);
    }
}
