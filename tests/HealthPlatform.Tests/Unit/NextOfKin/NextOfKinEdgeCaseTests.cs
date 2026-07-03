using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Appointments.EventHandlers;
using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Application.NextOfKin.AddNextOfKinContact;
using HealthPlatform.Application.NextOfKin.DispatchSystemEmergencyAlert;
using HealthPlatform.Application.NextOfKin.SendDoctorEmergencyAlert;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.NextOfKin;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.NextOfKin;

public sealed class NextOfKinEdgeCaseTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;
    private FakeTimeProvider _clock = null!;

    public async Task InitializeAsync()
    {
        _clock = new FakeTimeProvider(new DateTime(2026, 7, 3, 12, 0, 0, DateTimeKind.Utc));
        _host = new PatientRegistrationTestHost(
            nextOfKinEmergencyAlertNotifier: new ControllableNextOfKinEmergencyAlertNotifier { FailSms = true },
            nextOfKinChannelDeliveryGateway: new ControllableNextOfKinChannelDeliveryGateway { SucceedOnRetry = false },
            timeProvider: _clock);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Max_three_contacts_are_enforced_per_patient()
    {
        var patient = await RegisterPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;

        for (var index = 0; index < NextOfKinPolicies.MaxContactsPerPatient; index++)
        {
            await _host.Sender.Send(
                new AddNextOfKinContactCommand(
                    $"Contact {index}",
                    "Sibling",
                    $"+26377123456{index}",
                    null,
                    false),
                CancellationToken.None);
        }

        var contactCount = await _host.DbContext.NextOfKinContacts
            .CountAsync(contact => contact.PatientId == patient.Id);
        Assert.Equal(NextOfKinPolicies.MaxContactsPerPatient, contactCount);

        var ex = await Assert.ThrowsAsync<ConflictException>(() => _host.Sender.Send(
            new AddNextOfKinContactCommand(
                "Fourth Contact",
                "Cousin",
                "+263771234569",
                null,
                false),
            CancellationToken.None));

        Assert.Equal(NextOfKinErrorCodes.MaxContactsReached, ex.Code);

        var finalCount = await _host.DbContext.NextOfKinContacts
            .CountAsync(contact => contact.PatientId == patient.Id);
        Assert.Equal(NextOfKinPolicies.MaxContactsPerPatient, finalCount);
    }

    [Fact]
    public async Task Emergency_alert_with_no_next_of_kin_logs_alert_without_dispatch_or_retries()
    {
        var patient = await RegisterPatientAsync();

        var alert = await _host.Sender.Send(
            new DispatchSystemEmergencyAlertCommand(patient.Id, "No contacts on file."),
            CancellationToken.None);

        Assert.Equal(EmergencyAlertOverallStatus.NoContacts, alert.OverallStatus);
        Assert.Empty(alert.ContactDeliveries);
        Assert.Empty(_host.NextOfKinEmergencyAlertNotifier.Calls);

        var persistedAlert = await _host.DbContext.EmergencyAlerts
            .AsNoTracking()
            .SingleAsync(record => record.Id == alert.Id);
        Assert.Equal("No contacts on file.", persistedAlert.TriggerReason);
        Assert.Equal(EmergencyAlertOverallStatus.NoContacts, persistedAlert.OverallStatus);

        Assert.False(await _host.DbContext.EmergencyAlertContactDeliveries.AnyAsync());
        Assert.False(await _host.DbContext.NextOfKinNotificationDeliveries.AnyAsync());
    }

    [Fact]
    public async Task Doctor_emergency_alert_with_no_next_of_kin_logs_without_notifying()
    {
        var (appointmentId, doctor, patient) = await SeedConfirmedConsultationAsync();
        _host.CurrentUser.UserId = doctor.UserId;

        var alert = await _host.Sender.Send(
            new SendDoctorEmergencyAlertCommand(
                patient.Id,
                appointmentId,
                "Patient distress with no emergency contacts."),
            CancellationToken.None);

        Assert.Equal(EmergencyAlertOverallStatus.NoContacts, alert.OverallStatus);
        Assert.Empty(alert.ContactDeliveries);
        Assert.Empty(_host.NextOfKinEmergencyAlertNotifier.Calls);
        Assert.False(await _host.DbContext.NextOfKinNotificationDeliveries.AnyAsync());
    }

    [Fact]
    public async Task Retry_exhaustion_logs_final_failed_delivery_status_and_updates_alert_aggregate()
    {
        var patient = await RegisterPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;

        await _host.Sender.Send(
            new AddNextOfKinContactCommand(
                "Exhaustion Kin",
                "Parent",
                "+263771234567",
                null,
                false),
            CancellationToken.None);

        var alert = await _host.Sender.Send(
            new DispatchSystemEmergencyAlertCommand(patient.Id, "Unrecoverable channel failure."),
            CancellationToken.None);

        var retryService = _host.GetRequiredService<INextOfKinNotificationRetryService>();
        for (var attempt = 0; attempt < NextOfKinPolicies.MaxNotificationRetries; attempt++)
        {
            _clock.SetUtcNow(_clock.UtcNow.Add(NextOfKinPolicies.NotificationRetryInterval));
            await retryService.ProcessDueRetriesAsync(CancellationToken.None);
        }

        var delivery = await _host.DbContext.NextOfKinNotificationDeliveries.SingleAsync();
        Assert.Equal(NextOfKinNotificationDeliveryStatus.FailedFinal, delivery.Status);
        Assert.Equal(NextOfKinPolicies.MaxNotificationRetries, delivery.RetryCount);
        Assert.NotNull(delivery.FinalizedAtUtc);

        var contactDelivery = await _host.DbContext.EmergencyAlertContactDeliveries.SingleAsync();
        Assert.Equal(EmergencyAlertChannelDeliveryStatus.Failed, contactDelivery.SmsStatus);
        Assert.Equal(EmergencyAlertChannelDeliveryStatus.Sent, contactDelivery.PushStatus);

        var persistedAlert = await _host.DbContext.EmergencyAlerts
            .AsNoTracking()
            .SingleAsync(record => record.Id == alert.Id);
        Assert.Equal(EmergencyAlertOverallStatus.PartiallyFailed, persistedAlert.OverallStatus);

        var extraProcessed = await retryService.ProcessDueRetriesAsync(CancellationToken.None);
        Assert.Equal(0, extraProcessed);
    }

    private async Task<Patient> RegisterPatientAsync()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Edge Case Patient",
                null,
                $"edge-case-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
    }

    private async Task<(Guid AppointmentId, Doctor Doctor, Patient Patient)> SeedConfirmedConsultationAsync()
    {
        var doctorRegistration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(doctorRegistration.DoctorId), CancellationToken.None);

        var doctor = await _host.DbContext.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleAsync(d => d.Id == doctorRegistration.DoctorId);

        var patient = await RegisterPatientAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var booking = await _host.Sender.Send(
            new BookAppointmentCommand(
                doctor.Id,
                doctor.AvailabilitySlots.First().Id,
                _clock.UtcNow.AddDays(1)),
            CancellationToken.None);

        await new ConfirmAppointmentOnPaymentCompletedNotificationHandler(
            _host.GetRequiredService<IAppointmentRepository>(),
            _host.GetRequiredService<IOutboxRepository>(),
            _host.GetRequiredService<IDomainEventPublisher>())
            .Handle(
                new PaymentCompletedNotification(booking.AppointmentId, Guid.CreateVersion7(), _clock.UtcNow),
                CancellationToken.None);

        return (booking.AppointmentId, doctor, patient);
    }
}
