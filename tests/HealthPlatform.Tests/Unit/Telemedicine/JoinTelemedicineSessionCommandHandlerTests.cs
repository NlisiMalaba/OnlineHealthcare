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
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Telemedicine;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Telemedicine;

public sealed class JoinTelemedicineSessionCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Handle_Returns_rtc_credentials_for_confirmed_virtual_appointment()
    {
        var (appointmentId, patientUserId, _) = await SeedConfirmedVirtualAppointmentAsync();
        _host.CurrentUser.UserId = patientUserId;

        var response = await _host.Sender.Send(
            new JoinTelemedicineSessionCommand(appointmentId, TelemedicineSessionMode.Audio),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, response.SessionId);
        Assert.Equal(appointmentId, response.AppointmentId);
        Assert.StartsWith("tm-", response.ChannelName, StringComparison.Ordinal);
        Assert.False(string.IsNullOrWhiteSpace(response.RtcToken));
        Assert.Equal(TelemedicineSessionMode.Audio, response.Mode);
        Assert.True(response.ExpiresAtUtc > DateTime.UtcNow);

        var session = await _host.DbContext.TelemedicineSessions.SingleAsync();
        Assert.Equal(TelemedicineSessionStatus.Active, session.Status);
        Assert.NotNull(session.StartedAtUtc);
    }

    [Fact]
    public async Task Handle_Allows_doctor_to_join_same_session()
    {
        var (appointmentId, _, doctorUserId) = await SeedConfirmedVirtualAppointmentAsync();
        _host.CurrentUser.UserId = doctorUserId;

        var response = await _host.Sender.Send(
            new JoinTelemedicineSessionCommand(appointmentId, null),
            CancellationToken.None);

        Assert.NotEqual(0u, response.Uid);
        Assert.StartsWith("dev:", response.RtcToken, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Handle_Switches_mode_when_participant_joins_with_new_mode()
    {
        var (appointmentId, patientUserId, doctorUserId) = await SeedConfirmedVirtualAppointmentAsync();
        _host.CurrentUser.UserId = patientUserId;

        await _host.Sender.Send(
            new JoinTelemedicineSessionCommand(appointmentId, TelemedicineSessionMode.Video),
            CancellationToken.None);

        _host.CurrentUser.UserId = doctorUserId;

        var response = await _host.Sender.Send(
            new JoinTelemedicineSessionCommand(appointmentId, TelemedicineSessionMode.Chat),
            CancellationToken.None);

        Assert.Equal(TelemedicineSessionMode.Chat, response.Mode);

        var session = await _host.DbContext.TelemedicineSessions.SingleAsync();
        Assert.Equal(TelemedicineSessionMode.Chat, session.Mode);
    }

    [Theory]
    [InlineData(TelemedicineSessionMode.Audio)]
    [InlineData(TelemedicineSessionMode.Chat)]
    public async Task Handle_Applies_requested_mode_on_initial_join(TelemedicineSessionMode mode)
    {
        var (appointmentId, patientUserId, _) = await SeedConfirmedVirtualAppointmentAsync();
        _host.CurrentUser.UserId = patientUserId;

        var response = await _host.Sender.Send(
            new JoinTelemedicineSessionCommand(appointmentId, mode),
            CancellationToken.None);

        Assert.Equal(mode, response.Mode);
    }

    private async Task<(Guid AppointmentId, Guid PatientUserId, Guid DoctorUserId)> SeedConfirmedVirtualAppointmentAsync()
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
                "Join Patient",
                null,
                $"join-{Guid.NewGuid():N}@example.com",
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

        var confirmHandler = new ConfirmAppointmentOnPaymentCompletedNotificationHandler(
            _host.GetRequiredService<IAppointmentRepository>(),
            _host.GetRequiredService<IOutboxRepository>(),
            _host.GetRequiredService<IDomainEventPublisher>());

        await confirmHandler.Handle(
            new PaymentCompletedNotification(booking.AppointmentId, Guid.CreateVersion7(), DateTime.UtcNow),
            CancellationToken.None);

        var sessionHandler = new CreateTelemedicineSessionOnAppointmentConfirmedNotificationHandler(
            _host.GetRequiredService<IAppointmentRepository>(),
            _host.GetRequiredService<IDoctorRepository>(),
            _host.GetRequiredService<ITelemedicineSessionRepository>(),
            _host.GetRequiredService<IRtcProviderResolver>(),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<CreateTelemedicineSessionOnAppointmentConfirmedNotificationHandler>.Instance);

        var appointment = await _host.DbContext.Appointments.SingleAsync(x => x.Id == booking.AppointmentId);

        await sessionHandler.Handle(
            new AppointmentConfirmedNotification(
                appointment.Id,
                appointment.PatientId,
                appointment.DoctorId,
                appointment.ScheduledAtUtc,
                DateTime.UtcNow,
                DateTime.UtcNow),
            CancellationToken.None);

        return (booking.AppointmentId, patient.UserId, doctor.UserId);
    }
}
