using HealthPlatform.API.Controllers;
using HealthPlatform.API.Requests.Telemedicine;
using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Appointments.EventHandlers;
using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Telemedicine;
using HealthPlatform.Application.Telemedicine.EventHandlers;
using HealthPlatform.Application.Telemedicine.JoinSession;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Telemedicine;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Telemedicine;

public sealed class TelemedicineSessionsControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Join_endpoint_returns_rtc_credentials()
    {
        var doctorRegistration = await _host.Sender.Send(
            TelemedicineTestData.CreateVirtualDoctorCommand(),
            CancellationToken.None);

        var doctor = await _host.DbContext.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleAsync(d => d.Id == doctorRegistration.DoctorId);

        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Join Endpoint Patient",
                null,
                $"join-endpoint-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.OrderByDescending(p => p.CreatedAtUtc).FirstAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var booking = await _host.Sender.Send(
            new BookAppointmentCommand(
                doctor.Id,
                doctor.AvailabilitySlots.Single().Id,
                DateTime.UtcNow.AddDays(1)),
            CancellationToken.None);

        await ConfirmAndCreateSessionAsync(booking.AppointmentId);

        var controller = new TelemedicineSessionsController(_host.Sender);
        var result = await controller.JoinAsync(
            booking.AppointmentId,
            new JoinTelemedicineSessionRequest { Mode = TelemedicineSessionMode.Video },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<JoinTelemedicineSessionDto>(ok.Value);
        Assert.Equal(booking.AppointmentId, payload.AppointmentId);
        Assert.False(string.IsNullOrWhiteSpace(payload.RtcToken));
    }

    private async Task ConfirmAndCreateSessionAsync(Guid appointmentId)
    {
        var confirmHandler = new ConfirmAppointmentOnPaymentCompletedNotificationHandler(
            _host.GetRequiredService<IAppointmentRepository>(),
            _host.GetRequiredService<IOutboxRepository>(),
            _host.GetRequiredService<IDomainEventPublisher>());

        await confirmHandler.Handle(
            new PaymentCompletedNotification(appointmentId, Guid.CreateVersion7(), DateTime.UtcNow),
            CancellationToken.None);

        var appointment = await _host.DbContext.Appointments.SingleAsync(x => x.Id == appointmentId);
        var sessionHandler = new CreateTelemedicineSessionOnAppointmentConfirmedNotificationHandler(
            _host.GetRequiredService<IAppointmentRepository>(),
            _host.GetRequiredService<IDoctorRepository>(),
            _host.GetRequiredService<ITelemedicineSessionRepository>(),
            _host.GetRequiredService<IRtcProviderResolver>(),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<CreateTelemedicineSessionOnAppointmentConfirmedNotificationHandler>.Instance);

        await sessionHandler.Handle(
            new AppointmentConfirmedNotification(
                appointment.Id,
                appointment.PatientId,
                appointment.DoctorId,
                appointment.ScheduledAtUtc,
                DateTime.UtcNow,
                DateTime.UtcNow),
            CancellationToken.None);
    }
}
