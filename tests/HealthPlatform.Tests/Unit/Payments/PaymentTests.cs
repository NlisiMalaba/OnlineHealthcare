using HealthPlatform.Domain.Payments;
using Xunit;

namespace HealthPlatform.Tests.Unit.Payments;

public sealed class PaymentTests
{
    [Fact]
    public void RecordCompletion_raises_payment_completed_domain_event_when_appointment_present()
    {
        var appointmentId = Guid.CreateVersion7();
        var payment = Payment.RecordCompletion(
            Guid.CreateVersion7(),
            5000,
            "usd",
            PaymentMethod.Card,
            PaymentGatewayType.Paystack,
            "ref_1",
            appointmentId,
            null,
            null,
            DateTime.UtcNow);

        var domainEvent = Assert.Single(payment.DomainEvents);
        Assert.Contains(appointmentId.ToString(), domainEvent.ToString());
        Assert.Equal(PaymentStatus.Completed, payment.Status);
        Assert.Equal("USD", payment.Currency);
    }

    [Fact]
    public void RecordCompletion_does_not_raise_domain_event_without_appointment()
    {
        var payment = Payment.RecordCompletion(
            Guid.CreateVersion7(),
            5000,
            "USD",
            PaymentMethod.CreditLine,
            PaymentGatewayType.Internal,
            null,
            null,
            Guid.CreateVersion7(),
            null,
            DateTime.UtcNow);

        Assert.Empty(payment.DomainEvents);
    }
}
