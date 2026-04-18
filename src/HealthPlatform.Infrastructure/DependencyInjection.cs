using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Security;
using HealthPlatform.Infrastructure.Jobs;
using HealthPlatform.Infrastructure.Outbox;
using HealthPlatform.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HealthPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<Aes256AtRestEncryptionOptions>(
            configuration.GetSection(Aes256AtRestEncryptionOptions.SectionName));
        services.AddSingleton<IAtRestEncryption, Aes256AtRestEncryption>();
        services.AddSingleton<IOutboxDomainEventDispatcher, NoOpOutboxDomainEventDispatcher>();

        var redis = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redis))
        {
            services.AddStackExchangeRedisCache(options => options.Configuration = redis);
        }

        services.AddTransient<OutboxProcessorJob>();
        services.AddTransient<ScheduledRemindersJob>();
        return services;
    }
}
