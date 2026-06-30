using HealthPlatform.Application.Payments;
using HealthPlatform.Infrastructure.Payments;
using Microsoft.Extensions.Options;
using Xunit;

namespace HealthPlatform.Tests.Unit.Payments;

public sealed class PaymentGatewayResolverTests
{
    [Fact]
    public void GetRequired_returns_registered_gateway_by_provider_name()
    {
        var stripe = new TestPaymentGateway(PaymentGatewayProviders.Stripe);
        var flutterwave = new TestPaymentGateway(PaymentGatewayProviders.Flutterwave);
        var resolver = new PaymentGatewayResolver(
            [stripe, flutterwave],
            Options.Create(new PaymentGatewaysOptions { ActiveProvider = PaymentGatewayProviders.Paystack }));

        Assert.Same(stripe, resolver.GetRequired("stripe"));
        Assert.Same(flutterwave, resolver.GetRequired("FLUTTERWAVE"));
    }

    [Fact]
    public void GetRequired_throws_when_gateway_is_not_registered()
    {
        var resolver = new PaymentGatewayResolver(
            [new TestPaymentGateway(PaymentGatewayProviders.Stripe)],
            Options.Create(new PaymentGatewaysOptions()));

        Assert.Throws<InvalidOperationException>(() => resolver.GetRequired(PaymentGatewayProviders.Mpesa));
    }

    [Fact]
    public void ActiveProvider_defaults_to_flutterwave_when_unconfigured()
    {
        var resolver = new PaymentGatewayResolver(
            [new TestPaymentGateway(PaymentGatewayProviders.Stripe)],
            Options.Create(new PaymentGatewaysOptions { ActiveProvider = string.Empty }));

        Assert.Equal(PaymentGatewayProviders.Flutterwave, resolver.ActiveProvider);
    }

    private sealed class TestPaymentGateway(string providerName) : IPaymentGateway
    {
        public string ProviderName => providerName;

        public Task<PaymentIntentResultDto> CreatePaymentIntentAsync(
            CreatePaymentIntentRequestDto request,
            CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<PaymentCaptureResultDto> CapturePaymentAsync(
            CapturePaymentRequestDto request,
            CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<PaymentWebhookParseResultDto> ParseWebhookAsync(
            PaymentWebhookParseRequestDto request,
            CancellationToken ct) =>
            throw new NotSupportedException();
    }
}
