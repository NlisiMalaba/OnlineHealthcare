using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Appointments.CancelAppointment;
using HealthPlatform.Application.Appointments.EventHandlers;
using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Unit.Appointments;

public sealed class CancelAppointmentCommandHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Early_cancellation_releases_slot_and_requests_refund()
    {
        var (appointmentId, slotId, _) = await SeedConfirmedAppointmentAsync(hoursUntilAppointment: 5);

        var slotHoldService = _host.GetRequiredService<ISlotHoldService>();
        await slotHoldService.TryHoldAsync(slotId, Guid.CreateVersion7(), TimeSpan.FromMinutes(10), CancellationToken.None);
        Assert.True(await slotHoldService.IsSlotHeldAsync(slotId, CancellationToken.None));

        _host.CurrentUser.UserId = (await _host.DbContext.Patients.SingleAsync()).UserId;

        var result = await _host.Sender.Send(
            new CancelAppointmentCommand(appointmentId, "Changed plans"),
            CancellationToken.None);

        Assert.True(result.IsEarlyCancellation);
        Assert.True(result.SlotReleased);
        Assert.True(result.RefundRequested);
        Assert.Equal(0m, result.AppliedLateCancellationRetentionPercent);
        Assert.False(await slotHoldService.IsSlotHeldAsync(slotId, CancellationToken.None));

        var appointment = await _host.DbContext.Appointments.SingleAsync(a => a.Id == appointmentId);
        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);

        var hasRefundEvent = await _host.DbContext.DomainEventOutbox
            .AsNoTracking()
            .AnyAsync(x => x.EventType.Contains("AppointmentRefundRequestedDomainEvent"));

        Assert.True(hasRefundEvent);
    }

    [Fact]
    public async Task Late_cancellation_applies_doctor_policy_without_refund()
    {
        var (appointmentId, slotId, _) = await SeedConfirmedAppointmentAsync(hoursUntilAppointment: 1);

        var slotHoldService = _host.GetRequiredService<ISlotHoldService>();
        await slotHoldService.TryHoldAsync(slotId, Guid.CreateVersion7(), TimeSpan.FromMinutes(10), CancellationToken.None);

        _host.CurrentUser.UserId = (await _host.DbContext.Patients.SingleAsync()).UserId;

        var result = await _host.Sender.Send(
            new CancelAppointmentCommand(appointmentId, null),
            CancellationToken.None);

        Assert.False(result.IsEarlyCancellation);
        Assert.False(result.SlotReleased);
        Assert.False(result.RefundRequested);
        Assert.Equal(100m, result.AppliedLateCancellationRetentionPercent);
        Assert.True(await slotHoldService.IsSlotHeldAsync(slotId, CancellationToken.None));

        var hasPolicyEvent = await _host.DbContext.DomainEventOutbox
            .AsNoTracking()
            .AnyAsync(x => x.EventType.Contains("AppointmentLateCancellationPolicyAppliedDomainEvent"));

        Assert.True(hasPolicyEvent);
    }

    [Fact]
    public async Task Cancelling_pending_payment_appointment_is_rejected()
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
                "Cancel Patient",
                null,
                $"cancel-{Guid.NewGuid():N}@example.com",
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

        var ex = await Assert.ThrowsAsync<DomainException>(() => _host.Sender.Send(
            new CancelAppointmentCommand(booking.AppointmentId, null),
            CancellationToken.None));

        Assert.Equal(AppointmentErrorCodes.AppointmentNotCancellable, ex.Code);
    }

    private async Task<(Guid AppointmentId, Guid SlotId, Guid PatientUserId)> SeedConfirmedAppointmentAsync(
        int hoursUntilAppointment)
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
                "Cancel Flow Patient",
                null,
                $"cancel-flow-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.SingleAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var slotId = doctor.AvailabilitySlots.First().Id;
        var booking = await _host.Sender.Send(
            new BookAppointmentCommand(
                doctor.Id,
                slotId,
                DateTime.UtcNow.AddHours(hoursUntilAppointment)),
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

        return (booking.AppointmentId, slotId, patient.UserId);
    }
}
