using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Payments;
using HealthPlatform.Application.Payments.Webhooks;
using HealthPlatform.Infrastructure.Payments;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Payments;

public sealed class ProcessPaymentWebhookCommandHandlerTests
{
    [Fact]
    public async Task Handler_rejects_invalid_webhook_signature()
    {
        var gateway = new Mock<IPaymentGateway>();
        gateway.Setup(g => g.ProviderName).Returns(PaymentGatewayProviders.Stripe);
        gateway.Setup(g => g.ParseWebhookAsync(It.IsAny<PaymentWebhookParseRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentWebhookParseResultDto(
                false,
                null,
                null,
                PaymentWebhookEventStatus.Ignored,
                null,
                null,
                null,
                null,
                null,
                null));

        var resolver = new Mock<IPaymentGatewayResolver>();
        resolver.Setup(r => r.GetRequired(PaymentGatewayProviders.Stripe)).Returns(gateway.Object);

        var handler = CreateHandler(resolver.Object, new InMemoryPaymentWebhookIdempotencyStore(), new Mock<IPaymentCompletionService>().Object, new Mock<IPaymentFailureService>().Object);

        await Assert.ThrowsAsync<AccessDeniedException>(() => handler.Handle(
            new ProcessPaymentWebhookCommand(
                PaymentGatewayProviders.Stripe,
                "{}",
                new Dictionary<string, string>()),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handler_marks_duplicate_webhook_events_as_duplicate()
    {
        var gateway = CreateSuccessfulGateway();
        var resolver = new Mock<IPaymentGatewayResolver>();
        resolver.Setup(r => r.GetRequired(PaymentGatewayProviders.Flutterwave)).Returns(gateway);

        var idempotency = new InMemoryPaymentWebhookIdempotencyStore();
        var completionService = new Mock<IPaymentCompletionService>();
        var failureService = new Mock<IPaymentFailureService>();
        var handler = CreateHandler(resolver.Object, idempotency, completionService.Object, failureService.Object);

        var command = new ProcessPaymentWebhookCommand(
            PaymentGatewayProviders.Flutterwave,
            "{}",
            new Dictionary<string, string> { ["verif-hash"] = "dev:test" });

        var first = await handler.Handle(command, CancellationToken.None);
        var second = await handler.Handle(command, CancellationToken.None);

        Assert.False(first.Duplicate);
        Assert.True(second.Duplicate);
        completionService.Verify(
            s => s.CompleteAsync(It.IsAny<CompletePaymentRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handler_completes_payment_for_appointment_webhook()
    {
        var appointmentId = Guid.CreateVersion7();
        var gateway = CreateSuccessfulGateway(appointmentId);
        var resolver = new Mock<IPaymentGatewayResolver>();
        resolver.Setup(r => r.GetRequired(PaymentGatewayProviders.Paystack)).Returns(gateway);

        var completionService = new Mock<IPaymentCompletionService>();
        completionService
            .Setup(s => s.CompleteAsync(It.IsAny<CompletePaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentCompletionResultDto(
                Guid.CreateVersion7(),
                "patients/receipt.txt",
                "file:///receipt.txt"));

        var failureService = new Mock<IPaymentFailureService>();
        var handler = CreateHandler(
            resolver.Object,
            new InMemoryPaymentWebhookIdempotencyStore(),
            completionService.Object,
            failureService.Object);

        var result = await handler.Handle(
            new ProcessPaymentWebhookCommand(
                PaymentGatewayProviders.Paystack,
                "{}",
                new Dictionary<string, string> { ["x-paystack-signature"] = "dev:test" }),
            CancellationToken.None);

        Assert.True(result.Accepted);
        completionService.Verify(
            s => s.CompleteAsync(
                It.Is<CompletePaymentRequest>(request => request.AppointmentId == appointmentId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handler_records_payment_failure_for_failed_appointment_webhook()
    {
        var appointmentId = Guid.CreateVersion7();
        var gateway = CreateFailedGateway(appointmentId);
        var resolver = new Mock<IPaymentGatewayResolver>();
        resolver.Setup(r => r.GetRequired(PaymentGatewayProviders.Stripe)).Returns(gateway);

        var failureService = new Mock<IPaymentFailureService>();
        failureService
            .Setup(s => s.RecordFailureAsync(It.IsAny<RecordPaymentFailureRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentFailureResultDto(Guid.CreateVersion7(), DateTime.UtcNow.AddMinutes(10)));

        var handler = CreateHandler(
            resolver.Object,
            new InMemoryPaymentWebhookIdempotencyStore(),
            new Mock<IPaymentCompletionService>().Object,
            failureService.Object);

        var result = await handler.Handle(
            new ProcessPaymentWebhookCommand(
                PaymentGatewayProviders.Stripe,
                "{}",
                new Dictionary<string, string> { ["Stripe-Signature"] = "dev:test" }),
            CancellationToken.None);

        Assert.True(result.Accepted);
        failureService.Verify(
            s => s.RecordFailureAsync(
                It.Is<RecordPaymentFailureRequest>(request => request.AppointmentId == appointmentId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static IPaymentGateway CreateFailedGateway(Guid? appointmentId = null)
    {
        var gateway = new Mock<IPaymentGateway>();
        gateway.Setup(g => g.ProviderName).Returns(PaymentGatewayProviders.Stripe);
        gateway.Setup(g => g.ParseWebhookAsync(It.IsAny<PaymentWebhookParseRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentWebhookParseResultDto(
                true,
                "evt_failed",
                "provider_failed",
                PaymentWebhookEventStatus.Failed,
                2500,
                "USD",
                appointmentId ?? Guid.CreateVersion7(),
                null,
                "card_declined",
                "Card was declined."));
        return gateway.Object;
    }

    private static IPaymentGateway CreateSuccessfulGateway(Guid? appointmentId = null)
    {
        var gateway = new Mock<IPaymentGateway>();
        gateway.Setup(g => g.ProviderName).Returns(PaymentGatewayProviders.Flutterwave);
        gateway.Setup(g => g.ParseWebhookAsync(It.IsAny<PaymentWebhookParseRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentWebhookParseResultDto(
                true,
                "evt_1",
                "provider_1",
                PaymentWebhookEventStatus.Completed,
                2500,
                "USD",
                appointmentId ?? Guid.CreateVersion7(),
                null,
                null,
                null));
        return gateway.Object;
    }

    private static ProcessPaymentWebhookCommandHandler CreateHandler(
        IPaymentGatewayResolver resolver,
        IPaymentWebhookIdempotencyStore idempotencyStore,
        IPaymentCompletionService completionService,
        IPaymentFailureService failureService)
    {
        var appointmentRepository = new Mock<HealthPlatform.Application.Appointments.IAppointmentRepository>();
        appointmentRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
                HealthPlatform.Domain.Appointments.Appointment.CreatePendingPayment(
                    Guid.CreateVersion7(),
                    Guid.CreateVersion7(),
                    Guid.CreateVersion7(),
                    DateTime.UtcNow.AddDays(1),
                    DateTime.UtcNow.AddMinutes(10)));

        return new(
            resolver,
            idempotencyStore,
            completionService,
            failureService,
            appointmentRepository.Object,
            new Mock<HealthPlatform.Application.PharmacyOrders.IMedicationOrderRepository>().Object,
            TimeProvider.System,
            NullLogger<ProcessPaymentWebhookCommandHandler>.Instance);
    }
}
