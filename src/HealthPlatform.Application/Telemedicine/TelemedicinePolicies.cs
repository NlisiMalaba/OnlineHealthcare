namespace HealthPlatform.Application.Telemedicine;

public static class TelemedicinePolicies
{
    public static readonly TimeSpan RtcTokenTtl = TimeSpan.FromHours(2);

    public static readonly TimeSpan DurationTickInterval = TimeSpan.FromSeconds(1);

    public const int MaxChatMessageLength = 2000;

    public const long MaxSharedFileBytes = 10 * 1024 * 1024;
}
