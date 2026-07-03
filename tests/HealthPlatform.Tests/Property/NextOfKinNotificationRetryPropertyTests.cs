using FsCheck.Xunit;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Application.NextOfKin.AddNextOfKinContact;
using HealthPlatform.Application.NextOfKin.DispatchSystemEmergencyAlert;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.NextOfKin;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Properties;

public sealed class NextOfKinNotificationRetryPropertyTests
{
    private static readonly DateTime ReferenceNowUtc = new(2026, 7, 3, 8, 0, 0, DateTimeKind.Utc);

    // Feature: online-healthcare-platform, Property 23: Next of Kin Notification Retry
    [Property(Arbitrary = [typeof(WellnessArbitraries)], MaxTest = 100)]
    public bool Failed_next_of_kin_notification_retries_at_most_three_times(
        NextOfKinNotificationRetryCase input) =>
        RunRetryInvariantAsync(input).GetAwaiter().GetResult();

    private static async Task<bool> RunRetryInvariantAsync(NextOfKinNotificationRetryCase input)
    {
        var notifier = new ControllableNextOfKinEmergencyAlertNotifier
        {
            FailSms = input.FailedChannel == NextOfKinRetryFailedChannel.Sms,
            FailPush = input.FailedChannel == NextOfKinRetryFailedChannel.Push
        };
        var gateway = new ControllableNextOfKinChannelDeliveryGateway
        {
            SucceedOnAttempt = input.SucceedsOnAttempt
        };
        var clock = new FakeTimeProvider(ReferenceNowUtc);
        await using var host = new PatientRegistrationTestHost(
            nextOfKinEmergencyAlertNotifier: notifier,
            nextOfKinChannelDeliveryGateway: gateway,
            timeProvider: clock);

        var patient = await RegisterPatientWithContactAsync(host);
        var alert = await host.Sender.Send(
            new DispatchSystemEmergencyAlertCommand(patient.Id, "Retry property emergency alert."),
            CancellationToken.None);

        var retryDelivery = await host.DbContext.NextOfKinNotificationDeliveries.SingleAsync();
        if (retryDelivery.Channel != ExpectedFailedChannel(input.FailedChannel)
            || retryDelivery.RetryCount != 0
            || retryDelivery.Status != NextOfKinNotificationDeliveryStatus.AwaitingRetry)
        {
            return false;
        }

        var retryService = host.GetRequiredService<INextOfKinNotificationRetryService>();
        for (var tick = 0; tick < NextOfKinPolicies.MaxNotificationRetries; tick++)
        {
            clock.SetUtcNow(clock.UtcNow.Add(NextOfKinPolicies.NotificationRetryInterval));
            await retryService.ProcessDueRetriesAsync(CancellationToken.None);
        }

        var finalDelivery = await host.DbContext.NextOfKinNotificationDeliveries.SingleAsync();
        if (gateway.AttemptCount > NextOfKinPolicies.MaxNotificationRetries)
        {
            return false;
        }

        if (!FinalDeliveryMatchesExpectedOutcome(input, finalDelivery, gateway.AttemptCount))
        {
            return false;
        }

        var extraProcessed = await retryService.ProcessDueRetriesAsync(CancellationToken.None);
        if (extraProcessed != 0)
        {
            return false;
        }

        return await AlertDeliveryReflectsFinalStatusAsync(host, alert.Id, input, finalDelivery.Status);
    }

    private static bool FinalDeliveryMatchesExpectedOutcome(
        NextOfKinNotificationRetryCase input,
        NextOfKinNotificationDelivery delivery,
        int attemptCount)
    {
        if (input.SucceedsOnAttempt <= NextOfKinPolicies.MaxNotificationRetries)
        {
            return attemptCount == input.SucceedsOnAttempt
                && delivery.Status == NextOfKinNotificationDeliveryStatus.Sent
                && delivery.RetryCount == input.SucceedsOnAttempt - 1
                && delivery.FinalizedAtUtc.HasValue;
        }

        return attemptCount == NextOfKinPolicies.MaxNotificationRetries
            && delivery.Status == NextOfKinNotificationDeliveryStatus.FailedFinal
            && delivery.RetryCount == NextOfKinPolicies.MaxNotificationRetries
            && delivery.FinalizedAtUtc.HasValue;
    }

    private static async Task<bool> AlertDeliveryReflectsFinalStatusAsync(
        PatientRegistrationTestHost host,
        Guid alertId,
        NextOfKinNotificationRetryCase input,
        NextOfKinNotificationDeliveryStatus finalStatus)
    {
        var contactDelivery = await host.DbContext.EmergencyAlertContactDeliveries
            .AsNoTracking()
            .SingleAsync(delivery => delivery.EmergencyAlertId == alertId);

        var expectedFailedChannelStatus = finalStatus == NextOfKinNotificationDeliveryStatus.Sent
            ? EmergencyAlertChannelDeliveryStatus.Sent
            : EmergencyAlertChannelDeliveryStatus.Failed;

        return input.FailedChannel switch
        {
            NextOfKinRetryFailedChannel.Sms =>
                contactDelivery.SmsStatus == expectedFailedChannelStatus
                && contactDelivery.PushStatus == EmergencyAlertChannelDeliveryStatus.Sent,
            NextOfKinRetryFailedChannel.Push =>
                contactDelivery.PushStatus == expectedFailedChannelStatus
                && contactDelivery.SmsStatus == EmergencyAlertChannelDeliveryStatus.Sent,
            _ => false
        };
    }

    private static NextOfKinNotificationChannel ExpectedFailedChannel(NextOfKinRetryFailedChannel failedChannel) =>
        failedChannel switch
        {
            NextOfKinRetryFailedChannel.Sms => NextOfKinNotificationChannel.Sms,
            NextOfKinRetryFailedChannel.Push => NextOfKinNotificationChannel.Push,
            _ => throw new ArgumentOutOfRangeException(nameof(failedChannel), failedChannel, null)
        };

    private static async Task<Patient> RegisterPatientWithContactAsync(PatientRegistrationTestHost host)
    {
        await host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Retry Property Patient",
                null,
                $"retry-property-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
        host.CurrentUser.UserId = patient.UserId;

        await host.Sender.Send(
            new AddNextOfKinContactCommand(
                "Retry Property Kin",
                "Parent",
                "+263771234567",
                null,
                false),
            CancellationToken.None);

        return patient;
    }
}
