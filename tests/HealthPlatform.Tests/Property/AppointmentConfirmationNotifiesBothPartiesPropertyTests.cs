using FsCheck.Xunit;
using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Appointments.EventHandlers;
using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Properties;

public sealed class AppointmentConfirmationNotifiesBothPartiesPropertyTests
{
    // Feature: online-healthcare-platform, Property 8: Appointment Confirmation Notifies Both Parties
    [Property(Arbitrary = [typeof(AppointmentConfirmationArbitraries)], MaxTest = 100)]
    public bool Confirmation_notifies_patient_and_doctor_user_ids(AppointmentConfirmationFlowCase flowCase) =>
        RunBothPartiesNotifiedInvariantAsync(flowCase).GetAwaiter().GetResult();

    private static async Task<bool> RunBothPartiesNotifiedInvariantAsync(AppointmentConfirmationFlowCase flowCase)
    {
        var notifier = new CapturingAppointmentConfirmationNotifier();
        await using var host = new PatientRegistrationTestHost(notifier);

        var doctorRegistration = await host.Sender.Send(
            flowCase.DoctorRegistration.ToCommand(),
            CancellationToken.None);

        var doctor = await host.DbContext.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleAsync(d => d.Id == doctorRegistration.DoctorId);

        await host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Property Test Patient",
                null,
                $"patient-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await host.DbContext.Patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstAsync();

        var slotId = doctor.AvailabilitySlots.First().Id;

        host.CurrentUser.UserId = patient.UserId;

        var booking = await host.Sender.Send(
            new BookAppointmentCommand(
                doctor.Id,
                slotId,
                DateTime.UtcNow.AddDays(flowCase.DaysUntilAppointment)),
            CancellationToken.None);

        var paymentHandler = new ConfirmAppointmentOnPaymentCompletedNotificationHandler(
            host.GetRequiredService<IAppointmentRepository>(),
            host.GetRequiredService<IOutboxRepository>(),
            host.GetRequiredService<IDomainEventPublisher>());

        await paymentHandler.Handle(
            new PaymentCompletedNotification(
                booking.AppointmentId,
                Guid.CreateVersion7(),
                DateTime.UtcNow),
            CancellationToken.None);

        if (notifier.Calls.Count != 1)
        {
            return false;
        }

        var call = notifier.Calls[0];
        return call.AppointmentId == booking.AppointmentId
            && call.PatientUserId == patient.UserId
            && call.DoctorUserId == doctor.UserId
            && call.PatientUserId != Guid.Empty
            && call.DoctorUserId != Guid.Empty
            && call.PatientUserId != call.DoctorUserId;
    }
}
