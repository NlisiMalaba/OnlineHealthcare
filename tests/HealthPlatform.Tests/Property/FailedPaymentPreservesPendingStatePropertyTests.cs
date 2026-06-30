using FsCheck.Xunit;
using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Payments;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Payments;
using HealthPlatform.Domain.Pharmacy;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Properties;

public sealed class FailedPaymentPreservesPendingStatePropertyTests
{
    // Feature: online-healthcare-platform, Property 17: Failed Payment Preserves Pending State
    [Property(Arbitrary = [typeof(PaymentFailureArbitraries)], MaxTest = 100)]
    public bool Failed_payment_keeps_target_pending_and_emits_failure_notification(
        PaymentFailureCase testCase) =>
        RunFailedPaymentInvariantAsync(testCase).GetAwaiter().GetResult();

    private static async Task<bool> RunFailedPaymentInvariantAsync(PaymentFailureCase testCase)
    {
        await using var host = new PatientRegistrationTestHost();

        await host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Failure Property Patient",
                null,
                $"failure-property-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await host.DbContext.Patients.SingleAsync();
        host.CurrentUser.UserId = patient.UserId;

        Guid? appointmentId = null;
        Guid? medicationOrderId = null;

        if (testCase.TargetKind == PaymentFailureTargetKind.Appointment)
        {
            var doctorRegistration = await host.Sender.Send(
                DoctorRegistrationTestData.CreateValidCommand(),
                CancellationToken.None);

            var doctor = await host.DbContext.Doctors
                .Include(d => d.AvailabilitySlots)
                .SingleAsync(d => d.Id == doctorRegistration.DoctorId);

            var booking = await host.Sender.Send(
                new BookAppointmentCommand(
                    doctor.Id,
                    doctor.AvailabilitySlots.First().Id,
                    DateTime.UtcNow.AddHours(6)),
                CancellationToken.None);

            appointmentId = booking.AppointmentId;
        }
        else
        {
            var order = MedicationOrder.Place(
                patient.Id,
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                "MED-PROP",
                "Property Test Medication",
                "500mg",
                "Daily",
                7,
                null,
                MedicationDeliveryType.Pickup,
                null);

            await host.DbContext.MedicationOrders.AddAsync(order);
            await host.DbContext.SaveChangesAsync();
            medicationOrderId = order.Id;
        }

        host.PaymentFailedNotifier.Notifications.Clear();

        var failedAtUtc = DateTime.UtcNow;
        var failureService = host.GetRequiredService<IPaymentFailureService>();
        await failureService.RecordFailureAsync(
            new RecordPaymentFailureRequest(
                patient.Id,
                testCase.AmountMinorUnits,
                "USD",
                PaymentMethod.Card,
                PaymentGatewayType.Stripe,
                $"ref_{Guid.NewGuid():N}",
                appointmentId,
                medicationOrderId,
                null,
                testCase.FailureCode,
                testCase.FailureMessage,
                failedAtUtc),
            CancellationToken.None);

        if (appointmentId is { } resolvedAppointmentId)
        {
            var appointment = await host.DbContext.Appointments
                .AsNoTracking()
                .SingleAsync(a => a.Id == resolvedAppointmentId);

            if (appointment.Status != AppointmentStatus.PendingPayment)
            {
                return false;
            }

            if (appointment.SlotHoldExpiresAtUtc <= failedAtUtc)
            {
                return false;
            }
        }

        if (medicationOrderId is { } resolvedOrderId)
        {
            var order = await host.DbContext.MedicationOrders
                .AsNoTracking()
                .SingleAsync(o => o.Id == resolvedOrderId);

            if (order.Status != MedicationOrderStatus.Pending
                || order.PaymentRetryExpiresAtUtc is null
                || order.PaymentRetryExpiresAtUtc <= failedAtUtc)
            {
                return false;
            }
        }

        var notification = host.PaymentFailedNotifier.Notifications.SingleOrDefault();
        return notification is not null
            && notification.PatientId == patient.Id
            && notification.FailureCode == testCase.FailureCode
            && notification.FailureMessage == testCase.FailureMessage
            && notification.RetentionExpiresAtUtc > failedAtUtc;
    }
}
