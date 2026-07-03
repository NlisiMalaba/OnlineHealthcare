using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Application.NextOfKin.AddNextOfKinContact;
using HealthPlatform.Application.NextOfKin.DispatchSystemEmergencyAlert;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.NextOfKin;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.NextOfKin;

public sealed class NextOfKinNotificationRetryServiceTests : IAsyncLifetime
{
    private ControllableNextOfKinEmergencyAlertNotifier _notifier = null!;
    private ControllableNextOfKinChannelDeliveryGateway _gateway = null!;
    private PatientRegistrationTestHost _host = null!;
    private FakeTimeProvider _clock = null!;

    public async Task InitializeAsync()
    {
        _notifier = new ControllableNextOfKinEmergencyAlertNotifier { FailSms = true };
        _gateway = new ControllableNextOfKinChannelDeliveryGateway { SucceedOnRetry = false };
        _clock = new FakeTimeProvider(new DateTime(2026, 7, 2, 10, 0, 0, DateTimeKind.Utc));
        _host = new PatientRegistrationTestHost(
            nextOfKinEmergencyAlertNotifier: _notifier,
            nextOfKinChannelDeliveryGateway: _gateway,
            timeProvider: _clock);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task DispatchAsync_schedules_retry_when_initial_channel_delivery_fails()
    {
        var patient = await RegisterPatientWithContactAsync();

        await _host.Sender.Send(
            new DispatchSystemEmergencyAlertCommand(patient.Id, "Patient unresponsive."),
            CancellationToken.None);

        var retryDelivery = await _host.DbContext.NextOfKinNotificationDeliveries.SingleAsync();
        Assert.Equal(NextOfKinNotificationDeliveryStatus.AwaitingRetry, retryDelivery.Status);
        Assert.Equal(NextOfKinNotificationChannel.Sms, retryDelivery.Channel);
        Assert.Equal(0, retryDelivery.RetryCount);
        Assert.Equal(_clock.UtcNow.Add(NextOfKinPolicies.NotificationRetryInterval), retryDelivery.NextRetryAtUtc);
    }

    [Fact]
    public async Task ProcessDueRetriesAsync_marks_delivery_failed_final_after_three_retries()
    {
        var patient = await RegisterPatientWithContactAsync();
        await _host.Sender.Send(
            new DispatchSystemEmergencyAlertCommand(patient.Id, "Critical event."),
            CancellationToken.None);

        var retryService = _host.GetRequiredService<INextOfKinNotificationRetryService>();
        _clock.SetUtcNow(_clock.UtcNow.Add(NextOfKinPolicies.NotificationRetryInterval));

        for (var attempt = 1; attempt <= NextOfKinPolicies.MaxNotificationRetries; attempt++)
        {
            var processed = await retryService.ProcessDueRetriesAsync(CancellationToken.None);
            Assert.Equal(1, processed);

            var delivery = await _host.DbContext.NextOfKinNotificationDeliveries.SingleAsync();
            if (attempt < NextOfKinPolicies.MaxNotificationRetries)
            {
                Assert.Equal(NextOfKinNotificationDeliveryStatus.AwaitingRetry, delivery.Status);
                Assert.Equal(attempt, delivery.RetryCount);
                _clock.SetUtcNow(_clock.UtcNow.Add(NextOfKinPolicies.NotificationRetryInterval));
            }
            else
            {
                Assert.Equal(NextOfKinNotificationDeliveryStatus.FailedFinal, delivery.Status);
                Assert.Equal(NextOfKinPolicies.MaxNotificationRetries, delivery.RetryCount);
                Assert.NotNull(delivery.FinalizedAtUtc);
            }
        }

        Assert.Equal(NextOfKinPolicies.MaxNotificationRetries, _gateway.AttemptCount);

        var finalProcessed = await retryService.ProcessDueRetriesAsync(CancellationToken.None);
        Assert.Equal(0, finalProcessed);
    }

    [Fact]
    public async Task ProcessDueRetriesAsync_updates_contact_delivery_when_retry_succeeds()
    {
        var patient = await RegisterPatientWithContactAsync();
        _gateway.SucceedOnRetry = true;

        await _host.Sender.Send(
            new DispatchSystemEmergencyAlertCommand(patient.Id, "Recoverable channel failure."),
            CancellationToken.None);

        _clock.SetUtcNow(_clock.UtcNow.Add(NextOfKinPolicies.NotificationRetryInterval));
        var processed = await _host.GetRequiredService<INextOfKinNotificationRetryService>()
            .ProcessDueRetriesAsync(CancellationToken.None);

        Assert.Equal(1, processed);

        var delivery = await _host.DbContext.NextOfKinNotificationDeliveries.SingleAsync();
        Assert.Equal(NextOfKinNotificationDeliveryStatus.Sent, delivery.Status);
        Assert.NotNull(delivery.FinalizedAtUtc);

        var contactDelivery = await _host.DbContext.EmergencyAlertContactDeliveries.SingleAsync();
        Assert.Equal(EmergencyAlertChannelDeliveryStatus.Sent, contactDelivery.SmsStatus);
        Assert.Equal(EmergencyAlertChannelDeliveryStatus.Sent, contactDelivery.PushStatus);
    }

    private async Task<Patient> RegisterPatientWithContactAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Retry Patient",
                null,
                $"retry-patient-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
        _host.CurrentUser.UserId = patient.UserId;

        await _host.Sender.Send(
            new AddNextOfKinContactCommand(
                "Retry Kin",
                "Parent",
                "+263771234567",
                null,
                false),
            CancellationToken.None);

        return patient;
    }
}
