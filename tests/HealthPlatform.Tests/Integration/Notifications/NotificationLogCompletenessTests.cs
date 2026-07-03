using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Notifications;
using HealthPlatform.Domain.Notifications;
using HealthPlatform.Infrastructure.Notifications.Routing;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Notifications;

public sealed class NotificationLogCompletenessTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task DispatchAsync_persists_log_entry_for_each_attempted_channel()
    {
        var patientUserId = Guid.CreateVersion7();
        var notifier = _host.GetRequiredService<IAppointmentConfirmationNotifier>();
        Assert.IsType<RoutingAppointmentConfirmationNotifier>(notifier);

        await notifier.NotifyAppointmentConfirmedAsync(
            patientUserId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateTime.UtcNow.AddDays(1),
            CancellationToken.None);

        var logs = await _host.DbContext.NotificationLogs
            .AsNoTracking()
            .Where(entry => entry.RecipientId == patientUserId)
            .ToListAsync();

        Assert.Equal(3, logs.Count);
        Assert.All(logs, entry => Assert.Equal(NotificationEventTypes.AppointmentConfirmed, entry.EventType));
        Assert.All(logs, entry => Assert.Equal("patient", entry.RecipientType));
        Assert.All(logs, entry => Assert.Equal(1, entry.Attempts));
        Assert.All(logs, entry => Assert.True(entry.SentAtUtc <= DateTime.UtcNow));
        Assert.Contains(logs, entry => entry.Channel == "push");
        Assert.Contains(logs, entry => entry.Channel == "email");
        Assert.Contains(logs, entry => entry.Channel == "sms");
        Assert.All(logs, entry => Assert.True(Enum.IsDefined(entry.Status)));
    }
}
