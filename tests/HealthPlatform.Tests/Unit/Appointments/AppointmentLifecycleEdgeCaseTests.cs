using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Appointments.AvailabilitySlots;
using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Appointments.CancelAppointment;
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

public sealed class AppointmentLifecycleEdgeCaseTests : IAsyncLifetime
{
    private FakeTimeProvider _timeProvider = null!;
    private PatientRegistrationTestHost _host = null!;

    public Task InitializeAsync()
    {
        _timeProvider = new FakeTimeProvider(new DateTime(2026, 6, 24, 10, 0, 0, DateTimeKind.Utc));
        _host = new PatientRegistrationTestHost(timeProvider: _timeProvider);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Expired_slot_hold_allows_another_patient_to_book_same_slot()
    {
        var (doctor, slotId) = await SeedDoctorWithSlotAsync();
        var patientOne = await SeedPatientAsync("hold-one");
        var patientTwo = await SeedPatientAsync("hold-two");
        var scheduledAtUtc = _timeProvider.UtcNow.AddDays(1);

        _host.CurrentUser.UserId = patientOne.UserId;
        await _host.Sender.Send(
            new BookAppointmentCommand(doctor.Id, slotId, scheduledAtUtc),
            CancellationToken.None);

        var slotHoldService = _host.GetRequiredService<ISlotHoldService>();
        Assert.True(await slotHoldService.IsSlotHeldAsync(slotId, CancellationToken.None));

        _timeProvider.SetUtcNow(_timeProvider.UtcNow.AddMinutes(11));
        Assert.False(await slotHoldService.IsSlotHeldAsync(slotId, CancellationToken.None));

        _host.CurrentUser.UserId = patientTwo.UserId;
        var secondBooking = await _host.Sender.Send(
            new BookAppointmentCommand(doctor.Id, slotId, scheduledAtUtc.AddHours(1)),
            CancellationToken.None);

        Assert.Equal("pending_payment", secondBooking.Status);
        Assert.Equal(2, await _host.DbContext.Appointments.CountAsync());
    }

    [Fact]
    public async Task Cancellation_exactly_two_hours_before_applies_late_policy_not_refund()
    {
        var scheduledAtUtc = _timeProvider.UtcNow.AddHours(2);
        var (appointmentId, _, patientUserId, _) = await SeedConfirmedAppointmentAsync(scheduledAtUtc);

        _host.CurrentUser.UserId = patientUserId;

        var result = await _host.Sender.Send(
            new CancelAppointmentCommand(appointmentId, null),
            CancellationToken.None);

        Assert.False(result.IsEarlyCancellation);
        Assert.False(result.RefundRequested);
        Assert.False(result.SlotReleased);
        Assert.Equal(100m, result.AppliedLateCancellationRetentionPercent);
    }

    [Fact]
    public async Task Cancellation_just_over_two_hours_before_requests_refund_and_releases_slot()
    {
        var scheduledAtUtc = _timeProvider.UtcNow.AddHours(2).AddMinutes(1);
        var (appointmentId, doctor, patientUserId, _) = await SeedConfirmedAppointmentAsync(scheduledAtUtc);
        var slotId = doctor.AvailabilitySlots.First().Id;

        var slotHoldService = _host.GetRequiredService<ISlotHoldService>();
        await slotHoldService.TryHoldAsync(slotId, Guid.CreateVersion7(), TimeSpan.FromMinutes(10), CancellationToken.None);

        _host.CurrentUser.UserId = patientUserId;

        var result = await _host.Sender.Send(
            new CancelAppointmentCommand(appointmentId, null),
            CancellationToken.None);

        Assert.True(result.IsEarlyCancellation);
        Assert.True(result.RefundRequested);
        Assert.True(result.SlotReleased);
        Assert.False(await slotHoldService.IsSlotHeldAsync(slotId, CancellationToken.None));
    }

    [Fact]
    public async Task Late_cancellation_uses_doctor_configured_retention_percent()
    {
        var scheduledAtUtc = _timeProvider.UtcNow.AddHours(1);
        var (appointmentId, _, patientUserId, doctorId) = await SeedConfirmedAppointmentAsync(scheduledAtUtc);

        var doctor = await _host.DbContext.Doctors.SingleAsync(d => d.Id == doctorId);
        _host.DbContext.Entry(doctor).Property(nameof(Doctor.LateCancellationRetentionPercent)).CurrentValue = 40m;
        await _host.DbContext.SaveChangesAsync();

        _host.CurrentUser.UserId = patientUserId;

        var result = await _host.Sender.Send(
            new CancelAppointmentCommand(appointmentId, null),
            CancellationToken.None);

        Assert.False(result.IsEarlyCancellation);
        Assert.Equal(40m, result.AppliedLateCancellationRetentionPercent);
    }

    [Fact]
    public async Task Reschedule_rejects_when_confirmed_appointment_already_exists_at_target_time()
    {
        var (doctor, slotId) = await SeedDoctorWithSlotAsync();
        var patientOne = await SeedPatientAsync("reschedule-one");
        var patientTwo = await SeedPatientAsync("reschedule-two");
        var targetTime = _timeProvider.UtcNow.AddDays(3);

        _host.CurrentUser.UserId = patientOne.UserId;
        var firstBooking = await _host.Sender.Send(
            new BookAppointmentCommand(doctor.Id, slotId, targetTime),
            CancellationToken.None);
        await ConfirmAppointmentAsync(firstBooking.AppointmentId);

        _timeProvider.SetUtcNow(_timeProvider.UtcNow.AddMinutes(11));

        _host.CurrentUser.UserId = patientTwo.UserId;
        var secondBooking = await _host.Sender.Send(
            new BookAppointmentCommand(doctor.Id, slotId, _timeProvider.UtcNow.AddDays(5)),
            CancellationToken.None);
        await ConfirmAppointmentAsync(secondBooking.AppointmentId);

        _host.CurrentUser.UserId = doctor.UserId;

        var ex = await Assert.ThrowsAsync<ConflictException>(() => _host.Sender.Send(
            new RescheduleAppointmentCommand(secondBooking.AppointmentId, slotId, targetTime),
            CancellationToken.None));

        Assert.Equal(AppointmentErrorCodes.SlotUnavailable, ex.Code);
    }

    [Fact]
    public async Task Reschedule_with_unchanged_slot_and_time_is_idempotent()
    {
        var scheduledAtUtc = _timeProvider.UtcNow.AddDays(2);
        var (appointmentId, doctor, _, _) = await SeedConfirmedAppointmentAsync(scheduledAtUtc);
        var slotId = doctor.AvailabilitySlots.First().Id;

        _host.CurrentUser.UserId = doctor.UserId;
        var result = await _host.Sender.Send(
            new RescheduleAppointmentCommand(appointmentId, slotId, scheduledAtUtc),
            CancellationToken.None);

        Assert.Null(result.PreviousScheduledAtUtc);
        Assert.Equal(scheduledAtUtc, result.ScheduledAtUtc);

        var hasRescheduleEvent = await _host.DbContext.DomainEventOutbox
            .AsNoTracking()
            .AnyAsync(x => x.EventType.Contains("AppointmentRescheduledDomainEvent"));

        Assert.False(hasRescheduleEvent);
    }

    [Fact]
    public async Task Reschedule_rejects_when_doctor_does_not_own_appointment()
    {
        var scheduledAtUtc = _timeProvider.UtcNow.AddDays(2);
        var (appointmentId, _, _, _) = await SeedConfirmedAppointmentAsync(scheduledAtUtc);

        var otherDoctorRegistration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        var otherDoctor = await _host.DbContext.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleAsync(d => d.Id == otherDoctorRegistration.DoctorId);

        _host.CurrentUser.UserId = otherDoctor.UserId;

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _host.Sender.Send(
            new RescheduleAppointmentCommand(
                appointmentId,
                otherDoctor.AvailabilitySlots.First().Id,
                scheduledAtUtc.AddDays(1)),
            CancellationToken.None));

        Assert.Equal(AppointmentErrorCodes.AppointmentNotFound, ex.Code);
    }

    private async Task ConfirmAppointmentAsync(Guid appointmentId)
    {
        await new ConfirmAppointmentOnPaymentCompletedNotificationHandler(
            _host.GetRequiredService<IAppointmentRepository>(),
            _host.GetRequiredService<IOutboxRepository>(),
            _host.GetRequiredService<IDomainEventPublisher>())
            .Handle(
                new PaymentCompletedNotification(appointmentId, Guid.CreateVersion7(), _timeProvider.UtcNow),
                CancellationToken.None);
    }

    private async Task<(Doctor Doctor, Guid SlotId)> SeedDoctorWithSlotAsync()
    {
        var registration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        var doctor = await _host.DbContext.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleAsync(d => d.Id == registration.DoctorId);

        return (doctor, doctor.AvailabilitySlots.First().Id);
    }

    private async Task<Patient> SeedPatientAsync(string suffix)
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                $"Patient {suffix}",
                null,
                $"patient-{suffix}-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        return await _host.DbContext.Patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstAsync();
    }

    private async Task<(Guid AppointmentId, Doctor Doctor, Guid PatientUserId, Guid DoctorId)>
        SeedConfirmedAppointmentAsync(DateTime scheduledAtUtc)
    {
        var (doctor, slotId) = await SeedDoctorWithSlotAsync();
        var patient = await SeedPatientAsync("confirmed");

        _host.CurrentUser.UserId = patient.UserId;
        var booking = await _host.Sender.Send(
            new BookAppointmentCommand(doctor.Id, slotId, scheduledAtUtc),
            CancellationToken.None);

        await ConfirmAppointmentAsync(booking.AppointmentId);

        return (booking.AppointmentId, doctor, patient.UserId, doctor.Id);
    }
}
