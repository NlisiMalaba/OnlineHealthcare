using HealthPlatform.API.HostedServices;
using HealthPlatform.API.Hubs;
using HealthPlatform.API.Realtime;
using HealthPlatform.Application.Telemedicine.Realtime;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace HealthPlatform.API.Realtime;

public static class TelemedicineRealtimeServiceCollectionExtensions
{
    public static IServiceCollection AddTelemedicineRealtime(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redis = configuration.GetConnectionString("Redis");
        var signalR = services.AddSignalR();

        if (!string.IsNullOrWhiteSpace(redis))
        {
            signalR.AddStackExchangeRedis(redis, options =>
            {
                options.Configuration.ChannelPrefix = RedisChannel.Literal("HealthPlatform:SignalR");
            });
        }

        services.AddScoped<ITelemedicineSessionParticipantService, TelemedicineSessionParticipantService>();
        services.AddSingleton<ITelemedicineRealtimeNotifier, SignalRTelemedicineRealtimeNotifier>();
        services.AddHostedService<TelemedicineSessionDurationTickerHostedService>();
        services.AddHostedService<TelemedicineReconnectionGraceHostedService>();

        return services;
    }
}
