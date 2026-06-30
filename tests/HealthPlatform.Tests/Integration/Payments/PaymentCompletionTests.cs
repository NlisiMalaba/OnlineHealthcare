using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Payments;
using HealthPlatform.Application.Payments.GetPaymentReceipt;
using HealthPlatform.Application.Payments.ListPatientTransactionHistory;
using HealthPlatform.Application.Payments.Webhooks;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Payments;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Xunit;

namespace HealthPlatform.Tests.Integration.Payments;

public sealed class PaymentCompletionTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Webhook_completion_generates_receipt_transaction_history_and_confirms_appointment()
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
                "Receipt Patient",
                null,
                $"receipt-patient-{Guid.NewGuid():N}@example.com",
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

        var webhookBody = JsonSerializer.Serialize(new
        {
            @event = "charge.success",
            data = new
            {
                id = "evt_completion_test",
                reference = "ref_completion_test",
                amount = 2500,
                currency = "USD",
                metadata = new Dictionary<string, string>
                {
                    [PaymentMetadataKeys.AppointmentId] = booking.AppointmentId.ToString()
                }
            }
        });

        var result = await _host.Sender.Send(
            new ProcessPaymentWebhookCommand(
                PaymentGatewayProviders.Paystack,
                webhookBody,
                new Dictionary<string, string> { ["x-paystack-signature"] = "dev:test" }),
            CancellationToken.None);

        Assert.True(result.Accepted);

        var payment = await _host.DbContext.Payments.SingleAsync();
        Assert.Equal(patient.Id, payment.PatientId);
        Assert.Equal(booking.AppointmentId, payment.AppointmentId);
        Assert.False(string.IsNullOrWhiteSpace(payment.ReceiptStorageKey));

        var receipt = await _host.Sender.Send(new GetPaymentReceiptQuery(payment.Id), CancellationToken.None);
        Assert.Equal(payment.Id, receipt.PaymentId);
        Assert.StartsWith("file:///", receipt.ReceiptReadUrl);

        var history = await _host.Sender.Send(new ListPatientTransactionHistoryQuery(), CancellationToken.None);
        var paymentEntry = Assert.Single(history, item => item.PaymentId == payment.Id);
        Assert.Equal(PatientTransactionCategory.ConsultationFee, paymentEntry.Category);
        Assert.NotNull(paymentEntry.ReceiptUrl);

        var appointment = await _host.DbContext.Appointments.SingleAsync(a => a.Id == booking.AppointmentId);
        Assert.Equal(AppointmentStatus.Confirmed, appointment.Status);
    }

    [Fact]
    public async Task Get_receipt_rejects_payment_owned_by_another_patient()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Owner Patient",
                null,
                $"owner-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var owner = await _host.DbContext.Patients.SingleAsync();
        var completionService = _host.GetRequiredService<IPaymentCompletionService>();
        var payment = await completionService.CompleteAsync(
            new CompletePaymentRequest(
                owner.Id,
                1500,
                "USD",
                PaymentMethod.Card,
                PaymentGatewayType.Stripe,
                "ref_owner",
                null,
                null,
                null,
                DateTime.UtcNow),
            CancellationToken.None);

        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Other Patient",
                null,
                $"other-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var other = await _host.DbContext.Patients
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstAsync();
        _host.CurrentUser.UserId = other.UserId;

        await Assert.ThrowsAsync<NotFoundException>(() => _host.Sender.Send(
            new GetPaymentReceiptQuery(payment.PaymentId),
            CancellationToken.None));
    }
}
