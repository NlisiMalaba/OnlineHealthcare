using FsCheck;
using FsCheck.Xunit;
using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Appointments.CancelAppointment;
using HealthPlatform.Application.Appointments.EventHandlers;
using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Properties;

public sealed class EarlyCancellationReleasesSlotPropertyTests
{
    // Feature: online-healthcare-platform, Property 9: Early Cancellation Releases Slot
    [Property(MaxTest = 100)]
    public bool Early_cancellation_releases_slot_and_emits_refund_event(PositiveInt rawHoursUntilAppointment)
    {
        var hoursUntilAppointment = Math.Clamp(rawHoursUntilAppointment.Get % 70 + 3, 3, 72);
        return RunEarlyCancellationInvariantAsync(hoursUntilAppointment).GetAwaiter().GetResult();
    }

    private static async Task<bool> RunEarlyCancellationInvariantAsync(int hoursUntilAppointment)
    {
        await using var host = new PatientRegistrationTestHost();
        var slotHoldService = host.GetRequiredService<ISlotHoldService>();

        var doctorRegistration = await host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        var doctor = await host.DbContext.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleAsync(d => d.Id == doctorRegistration.DoctorId);

        await host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Property Cancel Patient",
                null,
                $"property-cancel-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await host.DbContext.Patients.SingleAsync();
        host.CurrentUser.UserId = patient.UserId;

        var slotId = doctor.AvailabilitySlots.First().Id;
        var booking = await host.Sender.Send(
            new BookAppointmentCommand(
                doctor.Id,
                slotId,
                DateTime.UtcNow.AddHours(hoursUntilAppointment)),
            CancellationToken.None);

        await new ConfirmAppointmentOnPaymentCompletedNotificationHandler(
            host.GetRequiredService<IAppointmentRepository>(),
            host.GetRequiredService<IOutboxRepository>(),
            host.GetRequiredService<IDomainEventPublisher>())
            .Handle(
                new PaymentCompletedNotification(
                    booking.AppointmentId,
                    Guid.CreateVersion7(),
                    DateTime.UtcNow),
                CancellationToken.None);

        await slotHoldService.TryHoldAsync(slotId, patient.Id, TimeSpan.FromMinutes(10), CancellationToken.None);
        if (!await slotHoldService.IsSlotHeldAsync(slotId, CancellationToken.None))
        {
            return false;
        }

        var cancellation = await host.Sender.Send(
            new CancelAppointmentCommand(booking.AppointmentId, "Property early cancel"),
            CancellationToken.None);

        if (!cancellation.IsEarlyCancellation || !cancellation.SlotReleased || !cancellation.RefundRequested)
        {
            return false;
        }

        if (await slotHoldService.IsSlotHeldAsync(slotId, CancellationToken.None))
        {
            return false;
        }

        var anotherPatientCanHold = await slotHoldService.TryHoldAsync(
            slotId,
            Guid.CreateVersion7(),
            TimeSpan.FromMinutes(10),
            CancellationToken.None);

        if (!anotherPatientCanHold)
        {
            return false;
        }

        return await host.DbContext.DomainEventOutbox
            .AsNoTracking()
            .AnyAsync(x => x.EventType.Contains("AppointmentRefundRequestedDomainEvent"));
    }
}
