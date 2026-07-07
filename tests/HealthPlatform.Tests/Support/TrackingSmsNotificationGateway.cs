using HealthPlatform.Application.Notifications;

namespace HealthPlatform.Tests.Support;

public sealed class TrackingSmsNotificationGateway : ISmsNotificationGateway
{
    public int AttemptCount { get; private set; }

    public bool Succeeds { get; set; } = true;

    public string Provider => "tracking";

    public bool IsConfigured => true;

    public Task<bool> TrySendAsync(SmsNotificationDeliveryRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        AttemptCount++;
        return Task.FromResult(Succeeds);
    }
}

public sealed class TrackingNotificationChannelGatewayResolver(
    ControllableNotificationGatewayConfig config,
    TrackingSmsNotificationGateway smsGateway) : INotificationChannelGatewayResolver
{
    private readonly ControllablePushNotificationGateway _push = new(config);
    private readonly ControllableEmailNotificationGateway _email = new(config);

    public TrackingSmsNotificationGateway SmsGateway { get; } = smsGateway;

    public IPushNotificationGateway ResolvePush() => _push;

    public ISmsNotificationGateway ResolveSms() => SmsGateway;

    public IEmailNotificationGateway ResolveEmail() => _email;
}
