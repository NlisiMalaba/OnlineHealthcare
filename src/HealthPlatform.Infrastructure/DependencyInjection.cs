using HealthPlatform.Infrastructure.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace HealthPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddTransient<OutboxProcessorJob>();
        services.AddTransient<ScheduledRemindersJob>();
        return services;
    }
}
