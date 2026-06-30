using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Payments;
using HealthPlatform.Application.Storage;
using HealthPlatform.Domain.Events;
using HealthPlatform.Domain.Payments;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Payments;

public sealed class PaymentCompletionServiceTests
{
    [Fact]
    public async Task CompleteAsync_persists_payment_receipt_and_publishes_domain_event_for_appointment()
    {
        var appointmentId = Guid.CreateVersion7();
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

        var storageService = new Mock<IStorageService>();
        storageService
            .Setup(s => s.UploadPaymentReceiptAsync(
                patientId,
                It.IsAny<Guid>(),
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StorageUploadResult("patients/receipt.txt", "text/plain"));

        storageService
            .Setup(s => s.GetSignedReadUrlAsync("patients/receipt.txt", It.IsAny<CancellationToken>()))
            .ReturnsAsync("file:///receipt.txt");

        var outboxRepository = new Mock<IOutboxRepository>();
        var domainEventPublisher = new Mock<IDomainEventPublisher>();
        var receiptGenerator = new Mock<IPaymentReceiptGenerator>();
        receiptGenerator.Setup(g => g.Generate(It.IsAny<Payment>())).Returns([1, 2, 3]);

        var service = new PaymentCompletionService(
            paymentRepository.Object,
            receiptGenerator.Object,
            storageService.Object,
            outboxRepository.Object,
            domainEventPublisher.Object);

        var completedAtUtc = DateTime.UtcNow;
        var result = await service.CompleteAsync(
            new CompletePaymentRequest(
                patientId,
                2500,
                "USD",
                PaymentMethod.Card,
                PaymentGatewayType.Stripe,
                "provider_123",
                appointmentId,
                null,
                null,
                completedAtUtc),
            CancellationToken.None);

        Assert.NotNull(savedPayment);
        Assert.Equal(patientId, savedPayment!.PatientId);
        Assert.Equal("patients/receipt.txt", savedPayment.ReceiptStorageKey);
        Assert.Equal(result.PaymentId, savedPayment.Id);

        outboxRepository.Verify(
            r => r.EnqueueAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
        domainEventPublisher.Verify(
            p => p.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
