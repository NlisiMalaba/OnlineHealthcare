using HealthPlatform.Application.Insurance;
using HealthPlatform.Infrastructure.Payments;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HealthPlatform.Infrastructure.Insurance;

public static class InsuranceServiceCollectionExtensions
{
    public static IServiceCollection AddInsuranceServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<InsurerApiOptions>(configuration.GetSection(InsurerApiOptions.SectionName));

        services.AddSingleton<IInsurerApiClientResolver, InsurerApiClientResolver>();
        services.AddScoped<IInsuranceClaimRepository, Persistence.Repositories.InsuranceClaimRepository>();
        services.AddScoped<IPatientInsurancePolicyRepository, Persistence.Repositories.PatientInsurancePolicyRepository>();
        services.AddScoped<IInsuranceClaimStatusPoller, InsuranceClaimStatusPoller>();

        var redis = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redis))
        {
            services.AddSingleton<IInsuranceClaimWebhookIdempotencyStore, RedisInsuranceClaimWebhookIdempotencyStore>();
        }
        else
        {
            services.AddSingleton<IInsuranceClaimWebhookIdempotencyStore, InMemoryInsuranceClaimWebhookIdempotencyStore>();
        }

        RegisterInsurerClients(services, configuration);
        return services;
    }

    private static void RegisterInsurerClients(IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(InsurerApiOptions.SectionName).Get<InsurerApiOptions>()
            ?? new InsurerApiOptions();

        var endpoints = options.Endpoints.Count > 0
            ? options.Endpoints
            :
            [
                new InsurerEndpointOptions { Code = "demo-insurer", Enabled = false }
            ];

        foreach (var endpoint in endpoints)
        {
            var insurerEndpoint = endpoint;
            var code = insurerEndpoint.Code.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(code))
            {
                continue;
            }

            insurerEndpoint.Code = code;
            services.AddSingleton<IInsurerApiClient>(sp =>
            {
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RestInsurerApiClient>>();
                return new RestInsurerApiClient(insurerEndpoint, factory, logger);
            });

            if (!string.IsNullOrWhiteSpace(insurerEndpoint.BaseUrl))
            {
                services.AddHttpClient(
                    RestInsurerApiClient.ClientName(code),
                    client =>
                    {
                        client.BaseAddress = new Uri(insurerEndpoint.BaseUrl!);
                        client.Timeout = TimeSpan.FromSeconds(insurerEndpoint.TimeoutSeconds);
                    })
                    .AddPolicyHandler(PaymentGatewayHttpPolicies.CreateResiliencePolicy());
            }
        }
    }
}
