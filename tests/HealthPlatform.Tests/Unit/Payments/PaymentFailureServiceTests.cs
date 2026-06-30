using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Payments;
using HealthPlatform.Domain.Payments;
using HealthPlatform.Domain.Payments.Events;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Payments;

public sealed class PaymentFailureServiceTests
{
    [Fact]
    public async Task RecordFailureAsync_publishes_payment_failed_domain_event()
    {
        var patientId = Guid.CreateVersion7();
        var paymentRepository = new Mock<IPaymentRepository>();
        Payment? savedPayment = null;

        paymentRepository
            .Setup(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Callback<Payment, CancellationToken>((payment, _) => savedPayment = payment)
            .Returns(Task.CompletedTask);

        paymentRepository
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var outboxRepository = new Mock<IOutboxRepository>();
        var domainEventPublisher = new Mock<IDomainEventPublisher>();
        var service = new PaymentFailureService(
            paymentRepository.Object,
            new Mock<HealthPlatform.Application.Appointments.IAppointmentRepository>().Object,
            new Mock<HealthPlatform.Application.PharmacyOrders.IMedicationOrderRepository>().Object,
            new Mock<HealthPlatform.Application.Appointments.ISlotHoldService>().Object,
            outboxRepository.Object,
            domainEventPublisher.Object,
            TimeProvider.System);

        var failedAtUtc = DateTime.UtcNow;
        var result = await service.RecordFailureAsync(
            new RecordPaymentFailureRequest(
                patientId,
                2500,
                "USD",
                PaymentMethod.Card,
                PaymentGatewayType.Stripe,
                "provider_failed",
                null,
                null,
                null,
                "card_declined",
                "Your card was declined.",
                failedAtUtc),
            CancellationToken.None);

        Assert.NotNull(savedPayment);
        Assert.Equal(PaymentStatus.Failed, savedPayment!.Status);
        Assert.Equal(result.PaymentId, savedPayment.Id);

        outboxRepository.Verify(
            r => r.EnqueueAsync(It.IsAny<PaymentFailedDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
        domainEventPublisher.Verify(
            p => p.PublishAsync(It.IsAny<PaymentFailedDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
