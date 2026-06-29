using HealthPlatform.Application.Payments;
using Polly;
using Polly.Extensions.Http;

namespace HealthPlatform.Infrastructure.Payments;

internal static class PaymentGatewayHttpPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> CreateResiliencePolicy() =>
        Policy.WrapAsync(CreateRetryPolicy(), CreateCircuitBreakerPolicy());

    private static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

    private static IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}
