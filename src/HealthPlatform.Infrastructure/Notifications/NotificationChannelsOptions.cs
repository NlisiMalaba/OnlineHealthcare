namespace HealthPlatform.Infrastructure.Notifications;

public sealed class NotificationChannelsOptions
{
    public const string SectionName = "Notifications:Channels";

    public PushChannelOptions Push { get; set; } = new();

    public SmsChannelOptions Sms { get; set; } = new();

    public EmailChannelOptions Email { get; set; } = new();
}

public sealed class PushChannelOptions
{
    public string ActiveProvider { get; set; } = Application.Notifications.NotificationChannelProviders.Logging;

    public FcmPushOptions Fcm { get; set; } = new();
}

public sealed class SmsChannelOptions
{
    public string ActiveProvider { get; set; } = Application.Notifications.NotificationChannelProviders.Logging;

    public TwilioSmsOptions Twilio { get; set; } = new();

    public AfricasTalkingSmsOptions AfricasTalking { get; set; } = new();
}

public sealed class EmailChannelOptions
{
    public string ActiveProvider { get; set; } = Application.Notifications.NotificationChannelProviders.Logging;

    public SendGridEmailOptions SendGrid { get; set; } = new();

    public SesEmailOptions Ses { get; set; } = new();
}

public abstract class NotificationProviderOptionsBase
{
    public bool Enabled { get; set; }

    public int TimeoutSeconds { get; set; } = 30;
}

public sealed class FcmPushOptions : NotificationProviderOptionsBase
{
    public string? ProjectId { get; set; }

    public string? ServiceAccountJson { get; set; }
}

public sealed class TwilioSmsOptions : NotificationProviderOptionsBase
{
    public string? AccountSid { get; set; }

    public string? AuthToken { get; set; }

    public string? FromNumber { get; set; }
}

public sealed class AfricasTalkingSmsOptions : NotificationProviderOptionsBase
{
    public string? ApiKey { get; set; }

    public string? Username { get; set; }

    public string? FromShortCode { get; set; }
}

public sealed class SendGridEmailOptions : NotificationProviderOptionsBase
{
    public string? ApiKey { get; set; }

    public string? FromEmail { get; set; }

    public string? FromName { get; set; }
}

public sealed class SesEmailOptions : NotificationProviderOptionsBase
{
    public string? AccessKeyId { get; set; }

    public string? SecretAccessKey { get; set; }

    public string? Region { get; set; }

    public string? FromEmail { get; set; }
}
