using HealthPlatform.Application.PharmacyOrders.Realtime;

namespace HealthPlatform.API.Realtime;

public static class PharmacyRealtimeServiceCollectionExtensions
{
    public static IServiceCollection AddPharmacyRealtime(this IServiceCollection services)
    {
        services.AddSingleton<IPharmacyOrderRealtimeNotifier, SignalRPharmacyOrderRealtimeNotifier>();
        return services;
    }
}
