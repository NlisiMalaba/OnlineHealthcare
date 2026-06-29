using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Payments;
using HealthPlatform.Application.Payments.Webhooks;
using HealthPlatform.Infrastructure.Payments;
using MediatR;
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

        var handler = CreateHandler(resolver.Object, new InMemoryPaymentWebhookIdempotencyStore(), new RecordingMediator());

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
        var mediator = new RecordingMediator();
        var handler = CreateHandler(resolver.Object, idempotency, mediator);

        var command = new ProcessPaymentWebhookCommand(
            PaymentGatewayProviders.Flutterwave,
            "{}",
            new Dictionary<string, string> { ["verif-hash"] = "dev:test" });

        var first = await handler.Handle(command, CancellationToken.None);
        var second = await handler.Handle(command, CancellationToken.None);

        Assert.False(first.Duplicate);
        Assert.True(second.Duplicate);
        Assert.Single(mediator.PublishedNotifications);
    }

    [Fact]
    public async Task Handler_publishes_payment_completed_notification_for_appointment()
    {
        var appointmentId = Guid.CreateVersion7();
        var gateway = CreateSuccessfulGateway(appointmentId);
        var resolver = new Mock<IPaymentGatewayResolver>();
        resolver.Setup(r => r.GetRequired(PaymentGatewayProviders.Paystack)).Returns(gateway);

        var mediator = new RecordingMediator();
        var handler = CreateHandler(
            resolver.Object,
            new InMemoryPaymentWebhookIdempotencyStore(),
            mediator);

        var result = await handler.Handle(
            new ProcessPaymentWebhookCommand(
                PaymentGatewayProviders.Paystack,
                "{}",
                new Dictionary<string, string> { ["x-paystack-signature"] = "dev:test" }),
            CancellationToken.None);

        Assert.True(result.Accepted);
        var notification = Assert.Single(mediator.PublishedNotifications) as PaymentCompletedNotification;
        Assert.NotNull(notification);
        Assert.Equal(appointmentId, notification.AppointmentId);
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
        IMediator mediator) =>
        new(
            resolver,
            idempotencyStore,
            mediator,
            TimeProvider.System,
            NullLogger<ProcessPaymentWebhookCommandHandler>.Instance);

    private sealed class RecordingMediator : IMediator
    {
        public List<INotification> PublishedNotifications { get; } = [];

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            if (notification is INotification typed)
            {
                PublishedNotifications.Add(typed);
            }

            return Task.CompletedTask;
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            PublishedNotifications.Add(notification);
            return Task.CompletedTask;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest =>
            throw new NotSupportedException();

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(
            object request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
