using HealthPlatform.Application.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Notifications;

public sealed class NotificationChannelGatewayResolver(
    IOptions<NotificationChannelsOptions> options,
    IEnumerable<IPushNotificationGateway> pushGateways,
    IEnumerable<ISmsNotificationGateway> smsGateways,
    IEnumerable<IEmailNotificationGateway> emailGateways,
    ILogger<NotificationChannelGatewayResolver> logger) : INotificationChannelGatewayResolver
{
    private readonly NotificationChannelsOptions _options = options.Value;

    public IPushNotificationGateway ResolvePush() =>
        Resolve(_options.Push.ActiveProvider, pushGateways, NotificationChannelProviders.Logging);

    public ISmsNotificationGateway ResolveSms() =>
        Resolve(_options.Sms.ActiveProvider, smsGateways, NotificationChannelProviders.Logging);

    public IEmailNotificationGateway ResolveEmail() =>
        Resolve(_options.Email.ActiveProvider, emailGateways, NotificationChannelProviders.Logging);

    private TGateway Resolve<TGateway>(
        string activeProvider,
        IEnumerable<TGateway> gateways,
        string fallbackProvider)
        where TGateway : class
    {
        var gatewayList = gateways.ToList();
        var configured = gatewayList.FirstOrDefault(gateway =>
            GetProvider(gateway) == activeProvider && IsConfigured(gateway));

        if (configured is not null)
        {
            return configured;
        }

        var fallback = gatewayList.FirstOrDefault(gateway => GetProvider(gateway) == fallbackProvider)
            ?? gatewayList.First();

        if (activeProvider != fallbackProvider)
        {
            logger.LogDebug(
                "Notification provider {ActiveProvider} is not configured; using {FallbackProvider}.",
                activeProvider,
                GetProvider(fallback));
        }

        return fallback;
    }

    private static string GetProvider<TGateway>(TGateway gateway) =>
        gateway switch
        {
            IPushNotificationGateway push => push.Provider,
            ISmsNotificationGateway sms => sms.Provider,
            IEmailNotificationGateway email => email.Provider,
            _ => NotificationChannelProviders.Logging
        };

    private static bool IsConfigured<TGateway>(TGateway gateway) =>
        gateway switch
        {
            IPushNotificationGateway push => push.IsConfigured,
            ISmsNotificationGateway sms => sms.IsConfigured,
            IEmailNotificationGateway email => email.IsConfigured,
            _ => false
        };
}
