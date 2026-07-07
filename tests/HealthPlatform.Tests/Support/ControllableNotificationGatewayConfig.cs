namespace HealthPlatform.Tests.Support;

public sealed class ControllableNotificationGatewayConfig
{
    public bool PushSucceeds { get; set; } = true;

    public bool SmsSucceeds { get; set; } = true;

    public bool EmailSucceeds { get; set; } = true;
}
