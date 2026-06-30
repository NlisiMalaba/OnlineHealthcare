using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Payments;
using HealthPlatform.Domain.Payments.Events;
using Xunit;

namespace HealthPlatform.Tests.Unit.Payments;

public sealed class PaymentFailurePolicyTests
{
    [Fact]
    public void RetainPendingAfterPaymentFailure_extends_slot_hold_for_pending_appointment()
    {
        var failedAtUtc = DateTime.UtcNow;
        var appointment = Appointment.CreatePendingPayment(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            failedAtUtc.AddDays(1),
            failedAtUtc.AddMinutes(2));

        var retentionExpiresAtUtc = appointment.RetainPendingAfterPaymentFailure(
            failedAtUtc,
            PaymentPolicies.PendingRetentionWindow);

        Assert.Equal(AppointmentStatus.PendingPayment, appointment.Status);
        Assert.True(appointment.SlotHoldExpiresAtUtc >= failedAtUtc.Add(PaymentPolicies.PendingRetentionWindow));
    }

    [Fact]
    public void RecordFailure_raises_payment_failed_domain_event()
    {
        var payment = Payment.RecordFailure(
            Guid.CreateVersion7(),
            1500,
            "USD",
            PaymentMethod.Card,
            PaymentGatewayType.Paystack,
            "ref_failed",
            Guid.CreateVersion7(),
            null,
            null,
            "insufficient_funds",
            "Insufficient funds.",
            DateTime.UtcNow,
            DateTime.UtcNow.Add(PaymentPolicies.PendingRetentionWindow));

        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.IsType<PaymentFailedDomainEvent>(Assert.Single(payment.DomainEvents));
    }
}
