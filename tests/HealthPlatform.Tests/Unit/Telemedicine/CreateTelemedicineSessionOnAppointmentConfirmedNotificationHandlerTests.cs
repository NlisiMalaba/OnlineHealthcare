using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Appointments.EventHandlers;
using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Telemedicine;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Telemedicine.EventHandlers;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Telemedicine;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Telemedicine;

public sealed class CreateTelemedicineSessionOnAppointmentConfirmedNotificationHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Handle_Creates_session_for_virtual_appointment()
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
                "Telemedicine Patient",
                null,
                $"telemed-{Guid.NewGuid():N}@example.com",
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

        await ConfirmAppointmentAsync(booking.AppointmentId);

        var handler = new CreateTelemedicineSessionOnAppointmentConfirmedNotificationHandler(
            _host.GetRequiredService<IAppointmentRepository>(),
            _host.GetRequiredService<IDoctorRepository>(),
            _host.GetRequiredService<ITelemedicineSessionRepository>(),
            _host.GetRequiredService<IRtcProviderResolver>(),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<CreateTelemedicineSessionOnAppointmentConfirmedNotificationHandler>.Instance);

        var appointment = await _host.DbContext.Appointments.SingleAsync(x => x.Id == booking.AppointmentId);

        await handler.Handle(
            new AppointmentConfirmedNotification(
                appointment.Id,
                appointment.PatientId,
                appointment.DoctorId,
                appointment.ScheduledAtUtc,
                DateTime.UtcNow,
                DateTime.UtcNow),
            CancellationToken.None);

        var session = await _host.DbContext.TelemedicineSessions.SingleAsync();
        Assert.Equal(booking.AppointmentId, session.AppointmentId);
        Assert.Equal(TelemedicineSessionStatus.Waiting, session.Status);
        Assert.StartsWith("tm-", session.ChannelName, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Handle_Skips_physical_only_appointment()
    {
        var doctorRegistration = await _host.Sender.Send(
            TelemedicineTestData.CreatePhysicalOnlyDoctorCommand(),
            CancellationToken.None);

        var doctor = await _host.DbContext.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleAsync(d => d.Id == doctorRegistration.DoctorId);

        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Physical Patient",
                null,
                $"physical-{Guid.NewGuid():N}@example.com",
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

        await ConfirmAppointmentAsync(booking.AppointmentId);

        var handler = new CreateTelemedicineSessionOnAppointmentConfirmedNotificationHandler(
            _host.GetRequiredService<IAppointmentRepository>(),
            _host.GetRequiredService<IDoctorRepository>(),
            _host.GetRequiredService<ITelemedicineSessionRepository>(),
            _host.GetRequiredService<IRtcProviderResolver>(),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<CreateTelemedicineSessionOnAppointmentConfirmedNotificationHandler>.Instance);

        var appointment = await _host.DbContext.Appointments.SingleAsync(x => x.Id == booking.AppointmentId);

        await handler.Handle(
            new AppointmentConfirmedNotification(
                appointment.Id,
                appointment.PatientId,
                appointment.DoctorId,
                appointment.ScheduledAtUtc,
                DateTime.UtcNow,
                DateTime.UtcNow),
            CancellationToken.None);

        Assert.False(await _host.DbContext.TelemedicineSessions.AnyAsync());
    }

    private async Task ConfirmAppointmentAsync(Guid appointmentId)
    {
        var confirmHandler = new ConfirmAppointmentOnPaymentCompletedNotificationHandler(
            _host.GetRequiredService<IAppointmentRepository>(),
            _host.GetRequiredService<IOutboxRepository>(),
            _host.GetRequiredService<IDomainEventPublisher>());

        await confirmHandler.Handle(
            new PaymentCompletedNotification(appointmentId, Guid.CreateVersion7(), DateTime.UtcNow),
            CancellationToken.None);
    }
}
