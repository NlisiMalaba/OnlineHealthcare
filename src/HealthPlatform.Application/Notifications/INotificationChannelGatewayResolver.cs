namespace HealthPlatform.Application.Notifications;

public interface INotificationChannelGatewayResolver
{
    IPushNotificationGateway ResolvePush();

    ISmsNotificationGateway ResolveSms();

    IEmailNotificationGateway ResolveEmail();
}
