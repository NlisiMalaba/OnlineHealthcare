using HealthPlatform.Infrastructure.Payments;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Tests.Support;

internal static class PaymentGatewayTestSupport
{
    public static PaymentGatewaysOptions CreateDefaultOptions() => new();

    public static IOptions<PaymentGatewaysOptions> CreateOptions() =>
        Options.Create(CreateDefaultOptions());

    public static StripePaymentGateway CreateStripeGateway() =>
        new(new DefaultHttpClientFactory(), CreateOptions(), NullLogger<StripePaymentGateway>.Instance);

    public static FlutterwavePaymentGateway CreateFlutterwaveGateway() =>
        new(new DefaultHttpClientFactory(), CreateOptions(), NullLogger<FlutterwavePaymentGateway>.Instance);

    public static PaystackPaymentGateway CreatePaystackGateway() =>
        new(new DefaultHttpClientFactory(), CreateOptions(), NullLogger<PaystackPaymentGateway>.Instance);

    public static MpesaPaymentGateway CreateMpesaGateway() =>
        new(new DefaultHttpClientFactory(), CreateOptions(), NullLogger<MpesaPaymentGateway>.Instance);

    private sealed class DefaultHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }
}
