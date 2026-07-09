using HealthPlatform.API.HostedServices;
using HealthPlatform.Application.Queue.Realtime;

namespace HealthPlatform.API.Realtime;

public static class QueueRealtimeServiceCollectionExtensions
{
    public static IServiceCollection AddQueueRealtime(this IServiceCollection services)
    {
        services.AddSingleton<IQueueRealtimeNotifier, SignalRQueueRealtimeNotifier>();
        services.AddHostedService<QueueRealtimeTickerHostedService>();
        return services;
    }
}
