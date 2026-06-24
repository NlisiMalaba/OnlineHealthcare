using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Appointments.AvailabilitySlots;
using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Appointments.EventHandlers;
using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Appointments.RescheduleAppointment;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Appointments;

public sealed class RescheduleAppointmentCommandHandlerTests : IAsyncLifetime
{
    private CapturingAppointmentRescheduleNotifier _notifier = null!;
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _notifier = new CapturingAppointmentRescheduleNotifier();
        _host = new PatientRegistrationTestHost(appointmentRescheduleNotifier: _notifier);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Reschedule_updates_appointment_and_notifies_patient()
    {
        var (appointmentId, doctor, originalScheduledAt, _) = await SeedConfirmedAppointmentAsync();

        var newSlot = doctor.AvailabilitySlots.First();
        var newScheduledAt = originalScheduledAt.AddDays(1);
        _host.CurrentUser.UserId = doctor.UserId;

        var result = await _host.Sender.Send(
            new RescheduleAppointmentCommand(appointmentId, newSlot.Id, newScheduledAt),
            CancellationToken.None);

        Assert.Equal(newSlot.Id, result.SlotId);
        Assert.Equal(newScheduledAt, result.ScheduledAtUtc);
        Assert.Equal(originalScheduledAt, result.PreviousScheduledAtUtc);
        Assert.Equal("confirmed", result.Status);

        var appointment = await _host.DbContext.Appointments.SingleAsync(a => a.Id == appointmentId);
        Assert.Equal(newScheduledAt, appointment.ScheduledAtUtc);
        Assert.Null(appointment.ReminderSentAtUtc);

        Assert.Single(_notifier.Calls);
        Assert.Equal(newScheduledAt, _notifier.Calls[0].NewScheduledAtUtc);
        Assert.Equal(originalScheduledAt, _notifier.Calls[0].PreviousScheduledAtUtc);

        var hasRescheduleEvent = await _host.DbContext.DomainEventOutbox
            .AsNoTracking()
            .AnyAsync(x => x.EventType.Contains("AppointmentRescheduledDomainEvent"));

        Assert.True(hasRescheduleEvent);
    }

    [Fact]
    public async Task Reschedule_rejects_when_new_slot_is_held()
    {
        var (appointmentId, doctor, originalScheduledAt, _) = await SeedConfirmedAppointmentAsync();

        _host.CurrentUser.UserId = doctor.UserId;
        var newSlot = await _host.Sender.Send(
            new CreateDoctorAvailabilitySlotCommand(
                DayOfWeek.Wednesday,
                new TimeOnly(9, 0),
                new TimeOnly(12, 0),
                30,
                DoctorAppointmentType.Virtual),
            CancellationToken.None);

        var newScheduledAt = originalScheduledAt.AddDays(2);

        var slotHoldService = _host.GetRequiredService<ISlotHoldService>();
        await slotHoldService.TryHoldAsync(newSlot.Id, Guid.CreateVersion7(), TimeSpan.FromMinutes(10), CancellationToken.None);

        var ex = await Assert.ThrowsAsync<ConflictException>(() => _host.Sender.Send(
            new RescheduleAppointmentCommand(appointmentId, newSlot.Id, newScheduledAt),
            CancellationToken.None));

        Assert.Equal(AppointmentErrorCodes.SlotUnavailable, ex.Code);
        Assert.Empty(_notifier.Calls);
    }

    [Fact]
    public async Task Reschedule_rejects_pending_payment_appointment()
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
                "Reschedule Patient",
                null,
                $"reschedule-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.SingleAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var booking = await _host.Sender.Send(
            new BookAppointmentCommand(
                doctor.Id,
                doctor.AvailabilitySlots.First().Id,
                DateTime.UtcNow.AddHours(5)),
            CancellationToken.None);

        _host.CurrentUser.UserId = doctor.UserId;

        var ex = await Assert.ThrowsAsync<DomainException>(() => _host.Sender.Send(
            new RescheduleAppointmentCommand(
                booking.AppointmentId,
                doctor.AvailabilitySlots.First().Id,
                DateTime.UtcNow.AddHours(8)),
            CancellationToken.None));

        Assert.Equal(AppointmentErrorCodes.AppointmentNotReschedulable, ex.Code);
    }

    private async Task<(Guid AppointmentId, Doctor Doctor, DateTime OriginalScheduledAt, Guid PatientUserId)>
        SeedConfirmedAppointmentAsync()
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
                "Reschedule Flow Patient",
                null,
                $"reschedule-flow-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.SingleAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var slotId = doctor.AvailabilitySlots.First().Id;
        var originalScheduledAt = DateTime.UtcNow.AddHours(6);

        var booking = await _host.Sender.Send(
            new BookAppointmentCommand(doctor.Id, slotId, originalScheduledAt),
            CancellationToken.None);

        await new ConfirmAppointmentOnPaymentCompletedNotificationHandler(
            _host.GetRequiredService<IAppointmentRepository>(),
            _host.GetRequiredService<IOutboxRepository>(),
            _host.GetRequiredService<IDomainEventPublisher>())
            .Handle(
                new PaymentCompletedNotification(
                    booking.AppointmentId,
                    Guid.CreateVersion7(),
                    DateTime.UtcNow),
                CancellationToken.None);

        return (booking.AppointmentId, doctor, originalScheduledAt, patient.UserId);
    }
}
