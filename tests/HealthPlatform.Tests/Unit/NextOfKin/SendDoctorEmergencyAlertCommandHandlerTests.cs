using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Application.NextOfKin.AddNextOfKinContact;
using HealthPlatform.Application.NextOfKin.SendDoctorEmergencyAlert;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Appointments.EventHandlers;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.NextOfKin;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.NextOfKin;

public sealed class SendDoctorEmergencyAlertCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;
    private FakeTimeProvider _timeProvider = null!;

    public async Task InitializeAsync()
    {
        _timeProvider = new FakeTimeProvider(new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc));
        _host = new PatientRegistrationTestHost(timeProvider: _timeProvider);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Handle_dispatches_alert_to_all_contacts_during_confirmed_consultation()
    {
        var (appointmentId, doctor, patient) = await SeedConfirmedConsultationAsync();
        _host.CurrentUser.UserId = patient.UserId;

        await _host.Sender.Send(
            new AddNextOfKinContactCommand(
                "Emergency Kin",
                "Parent",
                "+263771234567",
                "kin@example.com",
                false),
            CancellationToken.None);

        _host.CurrentUser.UserId = doctor.UserId;
        var alert = await _host.Sender.Send(
            new SendDoctorEmergencyAlertCommand(
                patient.Id,
                appointmentId,
                "Patient experiencing acute distress."),
            CancellationToken.None);

        Assert.Equal(EmergencyAlertTriggerSource.Doctor, alert.TriggerSource);
        Assert.Equal(doctor.Id, alert.TriggeredByDoctorId);
        Assert.Equal(EmergencyAlertOverallStatus.Dispatched, alert.OverallStatus);
        Assert.Single(_host.NextOfKinEmergencyAlertNotifier.Calls);
        Assert.Single(_host.NextOfKinEmergencyAlertNotifier.Calls[0].ContactIds);
    }

    private async Task<(Guid AppointmentId, Domain.Identity.Doctor Doctor, Domain.Identity.Patient Patient)>
        SeedConfirmedConsultationAsync()
    {
        var doctorRegistration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(doctorRegistration.DoctorId), CancellationToken.None);

        var doctor = await _host.DbContext.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleAsync(d => d.Id == doctorRegistration.DoctorId);

        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Consultation Patient",
                null,
                $"consultation-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var scheduledAtUtc = _timeProvider.UtcNow.AddDays(1);
        var booking = await _host.Sender.Send(
            new BookAppointmentCommand(doctor.Id, doctor.AvailabilitySlots.First().Id, scheduledAtUtc),
            CancellationToken.None);

        await new ConfirmAppointmentOnPaymentCompletedNotificationHandler(
            _host.GetRequiredService<IAppointmentRepository>(),
            _host.GetRequiredService<IOutboxRepository>(),
            _host.GetRequiredService<IDomainEventPublisher>())
            .Handle(
                new PaymentCompletedNotification(booking.AppointmentId, Guid.CreateVersion7(), _timeProvider.UtcNow),
                CancellationToken.None);

        return (booking.AppointmentId, doctor, patient);
    }
}
