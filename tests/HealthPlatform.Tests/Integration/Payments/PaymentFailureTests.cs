using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Payments;
using HealthPlatform.Application.Payments.Webhooks;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Payments;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Xunit;

namespace HealthPlatform.Tests.Integration.Payments;

public sealed class PaymentFailureTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Failed_webhook_retains_pending_appointment_notifies_patient_and_records_failed_payment()
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
                "Failed Payment Patient",
                null,
                $"failed-payment-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.SingleAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var booking = await _host.Sender.Send(
            new BookAppointmentCommand(
                doctor.Id,
                doctor.AvailabilitySlots.First().Id,
                DateTime.UtcNow.AddHours(6)),
            CancellationToken.None);

        _host.PaymentFailedNotifier.Notifications.Clear();

        var webhookBody = JsonSerializer.Serialize(new
        {
            @event = "charge.completed",
            data = new
            {
                id = "evt_failed_test",
                status = "failed",
                currency = "USD",
                flw_ref = "FLW-FAILED",
                meta = new Dictionary<string, string>
                {
                    [PaymentMetadataKeys.AppointmentId] = booking.AppointmentId.ToString()
                }
            }
        });

        var result = await _host.Sender.Send(
            new ProcessPaymentWebhookCommand(
                PaymentGatewayProviders.Flutterwave,
                webhookBody,
                new Dictionary<string, string> { ["verif-hash"] = "dev:test" }),
            CancellationToken.None);

        Assert.True(result.Accepted);
        Assert.Equal(PaymentWebhookEventStatus.Failed, result.Status);

        var appointment = await _host.DbContext.Appointments.SingleAsync(a => a.Id == booking.AppointmentId);
        Assert.Equal(AppointmentStatus.PendingPayment, appointment.Status);
        Assert.True(appointment.SlotHoldExpiresAtUtc > DateTime.UtcNow);

        var payment = await _host.DbContext.Payments.SingleAsync();
        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Equal("failed", payment.FailureCode);
        Assert.NotNull(payment.RetentionExpiresAtUtc);

        var notification = Assert.Single(_host.PaymentFailedNotifier.Notifications);
        Assert.Equal(payment.Id, notification.PaymentId);
        Assert.Equal(booking.AppointmentId, notification.AppointmentId);
        Assert.Equal("failed", notification.FailureCode);
    }
}
