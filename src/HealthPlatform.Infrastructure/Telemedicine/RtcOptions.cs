namespace HealthPlatform.Infrastructure.Telemedicine;

public sealed class RtcOptions
{
    public const string SectionName = "Rtc";

    public string Provider { get; set; } = "Agora";

    public string? AgoraAppId { get; set; }

    public string? AgoraAppCertificate { get; set; }

    public string? TwilioAccountSid { get; set; }

    public string? TwilioApiKeySid { get; set; }

    public string? TwilioApiKeySecret { get; set; }

    public int TokenTtlSeconds { get; set; } = 7200;
}
