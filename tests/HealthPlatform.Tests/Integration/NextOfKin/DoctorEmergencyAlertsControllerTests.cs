using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.NextOfKin;
using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Appointments.EventHandlers;
using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Application.NextOfKin.AddNextOfKinContact;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.NextOfKin;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.NextOfKin;

public sealed class DoctorEmergencyAlertsControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;
    private FakeTimeProvider _timeProvider = null!;

    public async Task InitializeAsync()
    {
        _timeProvider = new FakeTimeProvider(new DateTime(2026, 7, 1, 14, 0, 0, DateTimeKind.Utc));
        _host = new PatientRegistrationTestHost(timeProvider: _timeProvider);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task SendAsync_returns_created_emergency_alert()
    {
        var (appointmentId, doctor, patient) = await SeedConfirmedConsultationAsync();
        _host.CurrentUser.UserId = patient.UserId;

        await _host.Sender.Send(
            new AddNextOfKinContactCommand(
                "Controller Kin",
                "Sibling",
                "+263771234568",
                null,
                true),
            CancellationToken.None);

        _host.CurrentUser.UserId = doctor.UserId;
        var controller = new DoctorEmergencyAlertsController(_host.Sender);
        var result = await controller.SendAsync(
            new SendDoctorEmergencyAlertRequest
            {
                PatientId = patient.Id,
                AppointmentId = appointmentId,
                TriggerReason = "Sudden loss of consciousness observed."
            },
            CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result.Result);
        var alert = Assert.IsType<EmergencyAlertDto>(created.Value);
        Assert.Equal(EmergencyAlertTriggerSource.Doctor, alert.TriggerSource);
        Assert.Equal(EmergencyAlertOverallStatus.Dispatched, alert.OverallStatus);
        Assert.Single(alert.ContactDeliveries);
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
                "Controller Emergency Patient",
                null,
                $"controller-emergency-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var booking = await _host.Sender.Send(
            new BookAppointmentCommand(
                doctor.Id,
                doctor.AvailabilitySlots.First().Id,
                _timeProvider.UtcNow.AddDays(1)),
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
