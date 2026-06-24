using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Appointments.EventHandlers;
using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Infrastructure.Appointments;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HealthPlatform.Tests.Unit.Appointments;

public sealed class AppointmentReminderDispatcherTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task DispatchDueReminders_notifies_patient_and_doctor_within_30_minute_window()
    {
        var (appointmentId, patientUserId, doctorUserId, scheduledAtUtc) =
            await SeedConfirmedAppointmentAsync(minutesUntilAppointment: 20);

        var notifier = new CapturingAppointmentReminderNotifier();
        var clock = new FakeTimeProvider(DateTime.UtcNow);
        var dispatcher = CreateDispatcher(notifier, clock);

        var dispatched = await dispatcher.DispatchDueRemindersAsync(CancellationToken.None);

        Assert.Equal(1, dispatched);
        Assert.Single(notifier.Calls);
        Assert.Equal(appointmentId, notifier.Calls[0].AppointmentId);
        Assert.Equal(patientUserId, notifier.Calls[0].PatientUserId);
        Assert.Equal(doctorUserId, notifier.Calls[0].DoctorUserId);
        Assert.Equal(scheduledAtUtc, notifier.Calls[0].ScheduledAtUtc);

        var appointment = await _host.DbContext.Appointments.SingleAsync(a => a.Id == appointmentId);
        Assert.NotNull(appointment.ReminderSentAtUtc);
    }

    [Fact]
    public async Task DispatchDueReminders_skips_appointments_outside_30_minute_window()
    {
        await SeedConfirmedAppointmentAsync(minutesUntilAppointment: 45);

        var notifier = new CapturingAppointmentReminderNotifier();
        var dispatcher = CreateDispatcher(notifier, new FakeTimeProvider(DateTime.UtcNow));

        var dispatched = await dispatcher.DispatchDueRemindersAsync(CancellationToken.None);

        Assert.Equal(0, dispatched);
        Assert.Empty(notifier.Calls);
    }

    [Fact]
    public async Task DispatchDueReminders_is_idempotent_after_reminder_sent()
    {
        await SeedConfirmedAppointmentAsync(minutesUntilAppointment: 15);

        var notifier = new CapturingAppointmentReminderNotifier();
        var dispatcher = CreateDispatcher(notifier, new FakeTimeProvider(DateTime.UtcNow));

        var firstRun = await dispatcher.DispatchDueRemindersAsync(CancellationToken.None);
        var secondRun = await dispatcher.DispatchDueRemindersAsync(CancellationToken.None);

        Assert.Equal(1, firstRun);
        Assert.Equal(0, secondRun);
        Assert.Single(notifier.Calls);
    }

    private AppointmentReminderDispatcher CreateDispatcher(
        CapturingAppointmentReminderNotifier notifier,
        FakeTimeProvider clock) =>
        new(
            clock,
            _host.GetRequiredService<IAppointmentRepository>(),
            _host.GetRequiredService<HealthPlatform.Application.Identity.IPatientRepository>(),
            _host.GetRequiredService<HealthPlatform.Application.Identity.IDoctorRepository>(),
            notifier,
            NullLogger<AppointmentReminderDispatcher>.Instance);

    private async Task<(Guid AppointmentId, Guid PatientUserId, Guid DoctorUserId, DateTime ScheduledAtUtc)>
        SeedConfirmedAppointmentAsync(int minutesUntilAppointment)
    {
        var doctorRegistration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        var doctor = await _host.DbContext.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleAsync(d => d.Id == doctorRegistration.DoctorId);

        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Reminder Patient",
                null,
                $"reminder-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstAsync();

        _host.CurrentUser.UserId = patient.UserId;

        var scheduledAtUtc = DateTime.UtcNow.AddMinutes(minutesUntilAppointment);
        var booking = await _host.Sender.Send(
            new BookAppointmentCommand(
                doctor.Id,
                doctor.AvailabilitySlots.First().Id,
                scheduledAtUtc),
            CancellationToken.None);

        var paymentHandler = new ConfirmAppointmentOnPaymentCompletedNotificationHandler(
            _host.GetRequiredService<IAppointmentRepository>(),
            _host.GetRequiredService<IOutboxRepository>(),
            _host.GetRequiredService<IDomainEventPublisher>());

        await paymentHandler.Handle(
            new PaymentCompletedNotification(
                booking.AppointmentId,
                Guid.CreateVersion7(),
                DateTime.UtcNow),
            CancellationToken.None);

        return (booking.AppointmentId, patient.UserId, doctor.UserId, scheduledAtUtc);
    }
}
