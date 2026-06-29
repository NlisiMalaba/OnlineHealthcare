using HealthPlatform.Application.Payments;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Payments;

public static class PaymentGatewayServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentGateways(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PaymentGatewaysOptions>(
            configuration.GetSection(PaymentGatewaysOptions.SectionName));

        RegisterGatewayClient<StripePaymentGateway>(services, "https://api.stripe.com/v1/");
        RegisterGatewayClient<FlutterwavePaymentGateway>(services, "https://api.flutterwave.com/v3/");
        RegisterGatewayClient<PaystackPaymentGateway>(services, "https://api.paystack.co/");
        RegisterGatewayClient<MpesaPaymentGateway>(services, "https://sandbox.safaricom.co.ke/");

        services.AddSingleton<IPaymentGateway, StripePaymentGateway>();
        services.AddSingleton<IPaymentGateway, FlutterwavePaymentGateway>();
        services.AddSingleton<IPaymentGateway, PaystackPaymentGateway>();
        services.AddSingleton<IPaymentGateway, MpesaPaymentGateway>();
        services.AddSingleton<IPaymentGatewayResolver, PaymentGatewayResolver>();

        var redis = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redis))
        {
            services.AddSingleton<IPaymentWebhookIdempotencyStore, RedisPaymentWebhookIdempotencyStore>();
        }
        else
        {
            services.AddSingleton<IPaymentWebhookIdempotencyStore, InMemoryPaymentWebhookIdempotencyStore>();
        }

        return services;
    }

    private static void RegisterGatewayClient<TGateway>(IServiceCollection services, string baseAddress)
        where TGateway : class
    {
        services.AddHttpClient(typeof(TGateway).Name, (serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<PaymentGatewaysOptions>>().Value;
                var timeoutSeconds = ResolveTimeoutSeconds(options);
                client.BaseAddress = new Uri(baseAddress);
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            })
            .AddPolicyHandler(PaymentGatewayHttpPolicies.CreateResiliencePolicy());
    }

    private static int ResolveTimeoutSeconds(PaymentGatewaysOptions options) =>
        new[]
        {
            options.Stripe.TimeoutSeconds,
            options.Flutterwave.TimeoutSeconds,
            options.Paystack.TimeoutSeconds,
            options.Mpesa.TimeoutSeconds
        }.Max();
}
