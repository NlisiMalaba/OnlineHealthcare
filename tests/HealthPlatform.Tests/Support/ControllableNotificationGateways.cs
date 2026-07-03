using HealthPlatform.Application.Notifications;

namespace HealthPlatform.Tests.Support;

public sealed class ControllablePushNotificationGateway(ControllableNotificationGatewayConfig config)
    : IPushNotificationGateway
{
    public string Provider => "controllable";

    public bool IsConfigured => true;

    public Task<bool> TrySendAsync(PushNotificationDeliveryRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(config.PushSucceeds);
    }
}

public sealed class ControllableSmsNotificationGateway(ControllableNotificationGatewayConfig config)
    : ISmsNotificationGateway
{
    public string Provider => "controllable";

    public bool IsConfigured => true;

    public Task<bool> TrySendAsync(SmsNotificationDeliveryRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(config.SmsSucceeds);
    }
}

public sealed class ControllableEmailNotificationGateway(ControllableNotificationGatewayConfig config)
    : IEmailNotificationGateway
{
    public string Provider => "controllable";

    public bool IsConfigured => true;

    public Task<bool> TrySendAsync(EmailNotificationDeliveryRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(config.EmailSucceeds);
    }
}

public sealed class ControllableNotificationChannelGatewayResolver(
    ControllableNotificationGatewayConfig config) : INotificationChannelGatewayResolver
{
    private readonly ControllablePushNotificationGateway _push = new(config);
    private readonly ControllableSmsNotificationGateway _sms = new(config);
    private readonly ControllableEmailNotificationGateway _email = new(config);

    public IPushNotificationGateway ResolvePush() => _push;

    public ISmsNotificationGateway ResolveSms() => _sms;

    public IEmailNotificationGateway ResolveEmail() => _email;
}
